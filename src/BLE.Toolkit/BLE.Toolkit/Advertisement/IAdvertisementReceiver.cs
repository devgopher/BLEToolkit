using Microsoft.Extensions.Hosting;

namespace BLE.Toolkit.Advertisement;

public interface IAdvertisementReceiver : IHostedService
{
    public event GotAdvertisementHandler AdvertisementReceived;
}