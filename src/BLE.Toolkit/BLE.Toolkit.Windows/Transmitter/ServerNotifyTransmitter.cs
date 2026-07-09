using BLE.Toolkit.Interfaces.Transmitter;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Transmitter;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class ServerNotifyTransmitter(IOptionsMonitor<TransmitterSettings> settings) : BasicBleTransmitter(settings)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override void InnerTransmit(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override void Transmit(byte[] data)
    {
        throw new NotImplementedException();
    }
}