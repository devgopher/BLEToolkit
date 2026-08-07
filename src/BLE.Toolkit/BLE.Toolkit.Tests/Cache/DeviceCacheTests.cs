using BLE.Toolkit.Cache;

namespace BLE.Toolkit.Tests.Cache;

public class DeviceCacheTests
{
    [Fact]
    public void Add_StoresAdvertisements()
    {
        var cache = new DeviceCache(TimeSpan.FromMinutes(1));
        var advertisement = BleAdvertisementFactory.Create(
            bluetoothAddress: 0xAABBCCDDEEFF,
            rssi: -50,
            localName: "sensor");

        cache.Add(advertisement);

        Assert.Same(advertisement, Assert.Single(cache).Value);
        Assert.True(cache.Contains(advertisement));
    }

    [Fact]
    public void EntriesExpire_AfterTimeout()
    {
        var now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cache = new DeviceCache(TimeSpan.FromSeconds(30), () => now);
        var advertisement = BleAdvertisementFactory.Create(bluetoothAddress: 1, localName: "device");

        cache.Add(advertisement);
        Assert.Single(cache);

        now = now.AddSeconds(31);

        Assert.Empty(cache);
        Assert.False(cache.Contains(advertisement));
    }

    [Fact]
    public void Add_NullAdvertisement_IsIgnored()
    {
        var cache = new DeviceCache(TimeSpan.FromMinutes(1));

        cache.Add(null!);

        Assert.Empty(cache);
    }
}
