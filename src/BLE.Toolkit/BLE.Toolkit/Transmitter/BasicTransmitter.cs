using BLE.Toolkit.Cache;
using BLE.Toolkit.Exceptions;
using BLE.Toolkit.Hosting;
using BLE.Toolkit.Interfaces.Transmitter;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Transmitter;

/// <summary>
///     Base transmitter that queues outgoing frames and transmits them via <see cref="InnerTransmitAsync" />.
/// </summary>
public abstract class BasicTransmitter(IOptionsMonitor<TransmitterSettings> settings, DeviceCache deviceCache)
    : ITransmitter
{
    private readonly BackgroundJob _transmitJob = new();

    private Queue<TransmitElement> TransmitQueue { get; } = new(100);


    /// <summary>
    ///     Enqueues the provided data for later transmission.
    /// </summary>
    /// <param name="data">The frame to transmit.</param>
    public virtual void Transmit(byte[] data)
    {
        ExecuteQueueFillStrategy(new TransmitElement(null, data));
    }

    public void Transmit(ulong bluetoothAddress, byte[] data)
    {
        ExecuteQueueFillStrategy(new TransmitElement(bluetoothAddress, data));
    }

    /// <summary>
    ///     Starts the transmit loop that dequeues and sends frames until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token used to stop the transmit loop.</param>
    /// <returns>A completed task.</returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _transmitJob.Start(TransmitLoopAsync, cancellationToken);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Stops the transmitter.
    /// </summary>
    /// <param name="cancellationToken">Token used for stop coordination.</param>
    /// <returns>A completed task.</returns>
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        return _transmitJob.StopAsync(cancellationToken);
    }

    private TimeSpan GetPeriod()
    {
        if (settings.CurrentValue.RateLimiting is { Enabled: true })
            return settings.CurrentValue.RateLimiting?.RatePeriod switch
            {
                RatePeriod.Second => TimeSpan.FromSeconds(1),
                RatePeriod.Minute => TimeSpan.FromMinutes(1),
                RatePeriod.Hour => TimeSpan.FromHours(1),
                RatePeriod.Day => TimeSpan.FromDays(1),
                _ => TimeSpan.FromSeconds(1)
            };

        return TimeSpan.FromSeconds(1);
    }

    private async Task TransmitLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // If there is queued data, send it using the protocol-specific transport.
            if (!TransmitQueue.TryDequeue(out var transmitElement))
                continue;

            if (transmitElement.BluetoothAddress != null)
            {
                if (await InnerTransmitAsync(transmitElement))
                    await DoRateLimitingAsync(cancellationToken);
            }
            else
            {
                if (deviceCache.Count == 0)
                {
                    TransmitQueue.Enqueue(transmitElement);
                    continue;
                }

                // broadcast
                foreach (var cached in deviceCache.ToArray())
                {
                    if (cached == null!) continue;
                    if (await InnerTransmitAsync(transmitElement with { BluetoothAddress = cached.BluetoothAddress }))
                        await DoRateLimitingAsync(cancellationToken);
                }
            }
        }
    }

    private async Task DoRateLimitingAsync(CancellationToken cancellationToken)
    {
        if (settings.CurrentValue.RateLimiting is not { Enabled: true })
            return;

        var delta = RateLimitingPause();
        await Task.Delay(delta.Add(TimeSpan.FromMilliseconds(1)), cancellationToken);
    }

    private TimeSpan RateLimitingPause()
    {
        var delta = TimeSpan.Zero;

        if (settings.CurrentValue.RateLimiting is { Enabled: true })
            delta = TimeSpan.FromMilliseconds(GetPeriod().TotalMilliseconds / settings.CurrentValue.RateLimiting.Limit);
        return delta;
    }

    /// <summary>
    ///     Transmits a single queued frame to the underlying BLE transport.
    ///     Implementations provide the actual sending logic (e.g., writing to a characteristic).
    /// </summary>
    /// <param name="transmitElement"></param>
    protected abstract Task<bool> InnerTransmitAsync(TransmitElement transmitElement);

    protected virtual void ExecuteQueueFillStrategy(TransmitElement transmitElement)
    {
        switch (settings.CurrentValue.QueueFilledStrategy)
        {
            case QueueFilledStrategy.DequeueLast:
                while (TransmitQueue.Count >= settings.CurrentValue.TransmitQueueSize)
                    TransmitQueue.TryDequeue(out _);
                break;
            case QueueFilledStrategy.ThrowException:
                if (TransmitQueue.Count >= settings.CurrentValue.TransmitQueueSize)
                    throw new QueueFillException("Transmit queue filled!");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        TransmitQueue.Enqueue(transmitElement);
    }

    // Queue that stores outgoing payloads until the transmitter loop processes them.
    protected record TransmitElement(ulong? BluetoothAddress, byte[] Data);
}
