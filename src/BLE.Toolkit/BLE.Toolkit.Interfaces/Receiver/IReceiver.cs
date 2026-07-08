using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Interfaces.Receiver;

public interface IReceiver : IHostedService
{
    public List<byte[]> Output { get; }
}