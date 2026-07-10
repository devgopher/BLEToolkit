using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

/// <summary>
/// Transmits data via BLE using GATT notifications on a server-side characteristic.
/// When subscribers (clients) are connected and have enabled notifications,
/// this transmitter pushes the payload to them without requiring explicit reads.
/// </summary>
public class ServerNotifyTransmitter : BasicBleTransmitter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerNotifyTransmitter"/> class.
    /// </summary>
    /// <param name="settings">The transmitter settings monitored for changes.</param>
    public ServerNotifyTransmitter(IOptionsMonitor<TransmitterSettings> settings)
        : base(settings)
    {
    }

    /// <summary>
    /// Starts the BLE GATT server, begins advertising and advertisement publishing,
    /// then starts the base transmitter.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeGattServerAsync(cancellationToken);
        StartGattAdvertising();
        StartAdvertisementPublishing();
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Transmits data by notifying subscribers on the local transmit characteristic.
    /// Retries the notify operation using <c>ExecuteWithRetry</c>.
    /// </summary>
    /// <param name="data">The payload to transmit.</param>
    protected override void InnerTransmit(byte[] data)
    {
        // If the characteristic isn't available, nothing can be transmitted.
        if (LocalTransmitCharacteristic == null)
            return;

        ExecuteWithRetry(() =>
        {
            LocalTransmitCharacteristic
                .NotifyValueAsync(CreateBuffer(data)) // Convert payload to a BLE/WinRT buffer.
                .AsTask()
                .GetAwaiter()
                .GetResult();
        });
    }

    /// <summary>
    /// Creates an <see cref="IBuffer"/> containing the provided byte array.
    /// </summary>
    /// <param name="data">The payload bytes to place into the buffer.</param>
    /// <returns>An <see cref="IBuffer"/> with the serialized payload.</returns>
    private static IBuffer CreateBuffer(byte[] data)
    {
        var writer = new DataWriter();
        writer.WriteBytes(data);
        return writer.DetachBuffer();
    }
}