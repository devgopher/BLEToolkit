using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Windows.Cache;

/// <summary>
/// Cache of GATT characteristics results obtained via <c>GetCharacteristicsForUuidAsync</c>.
/// </summary>
public class GattCharacteristicsCache : CleanableList<CachedGattCharacteristics>
{
    public GattCharacteristicsCache(Func<DateTime>? utcNow = null)
        : base(utcNow)
    {
    }

    public GattCharacteristicsCache(CachePopStrategy<CachedGattCharacteristics> popStrategy,
        Func<DateTime>? utcNow = null)
        : base(popStrategy, utcNow)
    {
    }

    public CachedGattCharacteristics? FindByAddress(ulong bluetoothAddress) =>
        this.FirstOrDefault(entry => entry.Value.BluetoothAddress == bluetoothAddress)?.Value;
}
