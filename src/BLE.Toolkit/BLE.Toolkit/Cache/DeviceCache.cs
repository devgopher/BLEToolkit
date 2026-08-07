using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Cache;

/// <summary>
/// Bluetooth device cache
/// </summary>
public class DeviceCache : CleanableList<BleAdvertisement>
{
    public DeviceCache(TimeSpan timeout, Func<DateTime>? utcNow = null)
        : base(timeout, utcNow)
    {
    }

    public DeviceCache(TimeSpan timeout, CachePopStrategy<BleAdvertisement> popStrategy, Func<DateTime>? utcNow = null)
        : base(timeout, popStrategy, utcNow)
    {
    }
}
