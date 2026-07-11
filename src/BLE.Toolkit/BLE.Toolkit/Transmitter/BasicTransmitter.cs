using BLE.Toolkit.Exceptions;
using BLE.Toolkit.Interfaces.Transmitter;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Transmitter;

/// <summary>
///     Base transmitter that queues outgoing frames and transmits them via <see cref="InnerTransmit" />.
/// </summary>
public abstract class BasicTransmitter(IOptionsMonitor<TransmitterSettings> settings) : ITransmitter
{
    // Queue that stores outgoing payloads until the transmitter loop processes them.
    private Queue<byte[]> TransmitQueue { get; init; } = new(100);

    /// <summary>
    ///     Enqueues the provided data for later transmission.
    /// </summary>
    /// <param name="data">The frame to transmit.</param>
    public virtual void Transmit(byte[] data)
    {
        ExecuteQueueFillStrategy(data);
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
        else
            return TimeSpan.FromSeconds(1);
    }

    /// <summary>
    ///     Starts the transmit loop that dequeues and sends frames until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token used to stop the transmit loop.</param>
    /// <returns>A completed task.</returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        var delta = RateLimitingPause();

        while (!cancellationToken.IsCancellationRequested)
        {
            DoRateLimiting(delta);

            // If there is queued data, send it using the protocol-specific transport.
            if (TransmitQueue.TryDequeue(out var data))
                InnerTransmit(data);
        }

        return Task.CompletedTask;
    }

    private void DoRateLimiting(TimeSpan delta)
    {
        // Rate limiting
        if (settings.CurrentValue.RateLimiting is { Enabled: true }) 
            Thread.Sleep(delta.Add(TimeSpan.FromMilliseconds(1)));
    }

    private TimeSpan RateLimitingPause()
    {
        var delta = TimeSpan.Zero;

        if (settings.CurrentValue.RateLimiting is { Enabled: true })
            delta = TimeSpan.FromMilliseconds(GetPeriod().TotalMilliseconds / settings.CurrentValue.RateLimiting.Limit);
        return delta;
    }

    /// <summary>
    ///     Stops the transmitter.
    /// </summary>
    /// <param name="cancellationToken">Token used for stop coordination.</param>
    /// <returns>A completed task.</returns>
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Transmits a single queued frame to the underlying BLE transport.
    ///     Implementations provide the actual sending logic (e.g., writing to a characteristic).
    /// </summary>
    /// <param name="data">The frame to send.</param>
    protected abstract void InnerTransmit(byte[] data);
    
    protected virtual void ExecuteQueueFillStrategy(byte[] data)
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
        
        TransmitQueue.Enqueue(data);
    }
}