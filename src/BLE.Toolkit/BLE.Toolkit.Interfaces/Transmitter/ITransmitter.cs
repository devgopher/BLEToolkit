using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Interfaces.Transmitter;

public interface ITransmitter : IHostedService
{
    /// <summary>
    /// Broadcast transmit
    /// </summary>
    /// <param name="data"></param>
    public void Transmit(byte[] data);
    /// <summary>
    /// Transmit to a special device
    /// </summary>
    /// <param name="bluetoothAddress"></param>
    /// <param name="data"></param>
    public void Transmit(ulong bluetoothAddress, byte[] data);
}