using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Advertisement;

namespace BLE.Toolkit.Windows.Tests.Advertisement;

public class WindowsBleAdvertisementReceiverTests
{
    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotThrow()
    {
        var receiver = CreateReceiver(new AdvertisingSettings { Enabled = false });

        var exception = await Record.ExceptionAsync(() => receiver.StartAsync(CancellationToken.None));

        Assert.Null(exception);
        await receiver.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_WithoutStart_DoesNotThrow()
    {
        var receiver = CreateReceiver(new AdvertisingSettings { Enabled = false });

        var exception = await Record.ExceptionAsync(() => receiver.StopAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    private static WindowsBleAdvertisementReceiver CreateReceiver(AdvertisingSettings settings) =>
        new(
            new OptionsMonitorStub<AdvertisingSettings>(settings),
            new DeviceCache(TimeSpan.FromMinutes(1)));
}
