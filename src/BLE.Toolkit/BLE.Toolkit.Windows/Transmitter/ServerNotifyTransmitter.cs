using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class ServerNotifyTransmitter(IOptionsMonitor<TransmitterSettings> settings) : BasicBleTransmitter(settings)
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeGattServerAsync(cancellationToken);
        StartGattAdvertising();
        StartAdvertisementPublishing();
        await base.StartAsync(cancellationToken);
    }

    protected override void InnerTransmit(byte[] data)
    {
        if (LocalTransmitCharacteristic == null)
            return;

        ExecuteWithRetry(() =>
        {
            LocalTransmitCharacteristic
                .NotifyValueAsync(CreateBuffer(data))
                .AsTask()
                .GetAwaiter()
                .GetResult();
        });
    }

    private static IBuffer CreateBuffer(byte[] data)
    {
        var writer = new DataWriter();
        writer.WriteBytes(data);
        return writer.DetachBuffer();
    }
}
