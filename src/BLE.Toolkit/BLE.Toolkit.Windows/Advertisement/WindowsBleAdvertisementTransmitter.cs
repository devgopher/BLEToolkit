using BLE.Toolkit.Advertisement;
using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Windows.Advertisement;

public class WindowsBleAdvertisementTransmitter : IAdvertisementTransmitter
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}