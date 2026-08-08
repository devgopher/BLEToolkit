using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Cache;

/// <summary>
/// Bluetooth device cache
/// </summary>
public class DeviceCache : CleanableList<BleAdvertisement>
{
    public DeviceCache(Func<DateTime>? utcNow = null)
        : base(utcNow)
    {
    }

    public DeviceCache(CachePopStrategy<BleAdvertisement> popStrategy, Func<DateTime>? utcNow = null)
        : base(popStrategy, utcNow)
    {
    }
}
