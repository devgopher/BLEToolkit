using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Interfaces.Receiver;

public interface IReceiver : IHostedService
{
    public bool TryGetLast(out byte[] data);
}