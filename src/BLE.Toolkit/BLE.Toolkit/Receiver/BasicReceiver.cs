using BLE.Toolkit.Exceptions;
using BLE.Toolkit.Interfaces.Receiver;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Receiver;

/// <summary>
/// Base receiver implementation that pulls incoming BLE data chunks and stores them in a bounded queue.
/// </summary>
/// <param name="settings">Receiver settings monitor.</param>
public abstract class BasicReceiver(IOptionsMonitor<ReceiverSettings> settings) : IReceiver
{
    // Queue that holds recently received frames.
    private Queue<byte[]> ReceivedData { get; } = new(10);

    /// <summary>
    /// Attempts to get the next item from the internal queue.
    /// Note: This uses <c>TryDequeue</c>, so it removes the item from the queue.
    /// </summary>
    /// <param name="data">The dequeued data frame, if available.</param>
    /// <returns><c>true</c> if data was available; otherwise <c>false</c>.</returns>
    public virtual bool TryGetLast(out byte[] data)
    {
        return ReceivedData.TryDequeue(out data!);
    }

    /// <summary>
    /// Starts the receive loop and keeps filling the queue until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token used to stop the receive loop.</param>
    /// <returns>A completed task.</returns>
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Fetch the next chunk and apply the configured queue fill strategy.
            ExecuteQueueFillStrategy(GetDataChunk());
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the queue fill strategy when the queue reaches the configured capacity,
    /// then enqueues the provided data chunk.
    /// </summary>
    /// <param name="data">The incoming data chunk to enqueue.</param>
    protected virtual void ExecuteQueueFillStrategy(byte[] data)
    {
        switch (settings.CurrentValue.QueueFilledStrategy)
        {
            // When full, keep only the most recent frames by dequeuing until space is available.
            case QueueFilledStrategy.DequeueLast:
                while (ReceivedData.Count >= settings.CurrentValue.ReceiveQueueSize)
                    ReceivedData.TryDequeue(out _);
                break;

            // When full, throw an exception immediately.
            case QueueFilledStrategy.ThrowException:
                if (ReceivedData.Count >= settings.CurrentValue.ReceiveQueueSize)
                    throw new QueueFillException("Received queue filled!");
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Store the new incoming frame.
        ReceivedData.Enqueue(data);
    }

    /// <summary>
    /// Stops the receiver.
    /// </summary>
    /// <param name="cancellationToken">Token used for stop coordination.</param>
    /// <returns>A task representing the stop operation.</returns>
    public abstract Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the next incoming data chunk from the underlying BLE transport.
    /// </summary>
    /// <returns>The received data chunk.</returns>
    protected abstract byte[] GetDataChunk();
}
