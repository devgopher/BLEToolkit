using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Advertisement;

namespace BLE.Toolkit.Windows.Tests.Advertisement;

public class WindowsBleAdvertisementTransmitterTests
{
    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotThrow()
    {
        var transmitter = CreateTransmitter(new AdvertisingSettings
        {
            Enabled = false
        });

        var exception = await Record.ExceptionAsync(() => transmitter.StartAsync(CancellationToken.None));

        Assert.Null(exception);
        await transmitter.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WithoutPayload_ThrowsInvalidOperationException()
    {
        var transmitter = CreateTransmitter(new AdvertisingSettings
        {
            Enabled = true,
            ManufacturerData = null,
            DataSections = null
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transmitter.StartAsync(CancellationToken.None));

        Assert.Contains("ManufacturerData", exception.Message);
    }

    [Fact]
    public async Task StartAsync_OnlyReservedDataSections_ThrowsInvalidOperationException()
    {
        var transmitter = CreateTransmitter(new AdvertisingSettings
        {
            Enabled = true,
            DataSections =
            [
                new DataSection { DataType = 0x01, Data = [0x06] },
                new DataSection { DataType = 0x09, Data = [0x41] }
            ]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transmitter.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_OnlyManufacturerSpecificDataSection_ThrowsInvalidOperationException()
    {
        var transmitter = CreateTransmitter(new AdvertisingSettings
        {
            Enabled = true,
            DataSections =
            [
                new DataSection { DataType = 0xFF, Data = [0x01, 0x02] }
            ]
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transmitter.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StopAsync_WithoutStart_DoesNotThrow()
    {
        var transmitter = CreateTransmitter(new AdvertisingSettings { Enabled = false });

        var exception = await Record.ExceptionAsync(() => transmitter.StopAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    private static WindowsBleAdvertisementTransmitter CreateTransmitter(AdvertisingSettings settings) =>
        new(new OptionsMonitorStub<AdvertisingSettings>(settings));
}
