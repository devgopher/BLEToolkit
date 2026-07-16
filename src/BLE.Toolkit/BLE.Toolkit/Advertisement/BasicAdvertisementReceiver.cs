using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Advertisement;

public abstract class BasicAdvertisementReceiver : IAdvertisementReceiver
{
    public abstract Task StartAsync(CancellationToken cancellationToken);

    public abstract Task StopAsync(CancellationToken cancellationToken);

    public event GotAdvertisementHandler? AdvertisementReceived;

    protected virtual void OnAdvertisementReceived(BleAdvertisement advertisement)
    {
        AdvertisementReceived?.Invoke(advertisement);
    }
}