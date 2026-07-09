using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Interfaces.Transmitter;

public interface ITransmitter : IHostedService
{
    public void Transmit(byte[] data);
}