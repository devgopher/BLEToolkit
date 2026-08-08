using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Tests.Cache;

public class DeviceCacheTests
{
    [Fact]
    public void Add_StoresAdvertisements()
    {
        var cache = new DeviceCache();
        var advertisement = BleAdvertisementFactory.Create(
            bluetoothAddress: 0xAABBCCDDEEFF,
            rssi: -50,
            localName: "sensor");

        cache.Add(advertisement);

        Assert.Same(advertisement, Assert.Single(cache).Value);
        Assert.True(cache.Contains(advertisement));
    }

    [Fact]
    public void Add_NullAdvertisement_IsIgnored()
    {
        var cache = new DeviceCache();

        cache.Add(null!);

        Assert.Empty(cache);
    }
}
