using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Windows.Cache;

/// <summary>
/// Cache of GATT services results obtained via <c>GetGattServicesForUuidAsync</c>.
/// </summary>
public class GattServicesCache : CleanableList<CachedGattServices>
{
    public GattServicesCache(Func<DateTime>? utcNow = null)
        : base(utcNow)
    {
    }

    public GattServicesCache(CachePopStrategy<CachedGattServices> popStrategy, Func<DateTime>? utcNow = null)
        : base(popStrategy, utcNow)
    {
    }

    public CachedGattServices? FindByAddress(ulong bluetoothAddress) =>
        this.FirstOrDefault(entry => entry.Value.BluetoothAddress == bluetoothAddress)?.Value;
}
