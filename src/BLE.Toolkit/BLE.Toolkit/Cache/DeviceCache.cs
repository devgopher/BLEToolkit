using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Cache;

/// <summary>
/// Bluetooth device cache
/// </summary>
/// <param name="timeout"></param>
/// <param name="utcNow"></param>
public class DeviceCache(TimeSpan timeout, Func<DateTime>? utcNow = null)
    : ExpiredList<BleAdvertisement>(timeout, utcNow);