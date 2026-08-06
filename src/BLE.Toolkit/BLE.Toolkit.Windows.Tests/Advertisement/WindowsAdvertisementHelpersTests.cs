using Windows.Devices.Bluetooth.Advertisement;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Advertisement;

namespace BLE.Toolkit.Windows.Tests.Advertisement;

public class WindowsAdvertisementHelpersTests
{
    [Theory]
    [InlineData(0x01)]
    [InlineData(0x09)]
    [InlineData(0x16)]
    [InlineData(0x21)]
    [InlineData(0x3D)]
    public void IsReservedDataType_KnownReserved_ReturnsTrue(byte dataType)
    {
        Assert.True(WindowsAdvertisementHelpers.IsReservedDataType(dataType));
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x0B)]
    [InlineData(0x0C)]
    [InlineData(0xFF)]
    [InlineData(0x3E)]
    public void IsReservedDataType_NonReserved_ReturnsFalse(byte dataType)
    {
        Assert.False(WindowsAdvertisementHelpers.IsReservedDataType(dataType));
    }

    [Fact]
    public void IsManufacturerSpecificDataType_Only0xFF()
    {
        Assert.True(WindowsAdvertisementHelpers.IsManufacturerSpecificDataType(0xFF));
        Assert.False(WindowsAdvertisementHelpers.IsManufacturerSpecificDataType(0xFE));
    }

    [Fact]
    public void FilterPublishableDataSections_DropsReservedKeepsCustom()
    {
        var sections = new DataSection[]
        {
            new() { DataType = 0x01, Data = [0x06] },
            new() { DataType = 0xFF, Data = [0x01, 0x02] },
            new() { DataType = 0x09, Data = [0x41] },
            new() { DataType = 0xFE, Data = [0xAA] }
        };

        var filtered = WindowsAdvertisementHelpers.FilterPublishableDataSections(sections).ToArray();

        Assert.Equal(2, filtered.Length);
        Assert.Equal(0xFF, filtered[0].DataType);
        Assert.Equal(0xFE, filtered[1].DataType);
    }

    [Fact]
    public void HasValidPublisherPayload_WithManufacturerData_IsValid()
    {
        Assert.True(WindowsAdvertisementHelpers.HasValidPublisherPayload(true, []));
        Assert.True(WindowsAdvertisementHelpers.HasValidPublisherPayload(true, [0xFF]));
    }

    [Fact]
    public void HasValidPublisherPayload_CustomSection_IsValid()
    {
        Assert.True(WindowsAdvertisementHelpers.HasValidPublisherPayload(false, [0xFE]));
    }

    [Fact]
    public void HasValidPublisherPayload_OnlyManufacturerSectionType_IsInvalid()
    {
        Assert.False(WindowsAdvertisementHelpers.HasValidPublisherPayload(false, [0xFF]));
    }

    [Fact]
    public void HasValidPublisherPayload_Empty_IsInvalid()
    {
        Assert.False(WindowsAdvertisementHelpers.HasValidPublisherPayload(false, []));
    }

    [Theory]
    [InlineData(AdvertisingMode.Passive, BluetoothLEScanningMode.Passive)]
    [InlineData(AdvertisingMode.Active, BluetoothLEScanningMode.Active)]
    [InlineData(AdvertisingMode.Balanced, BluetoothLEScanningMode.Active)]
    public void MapScanningMode_MapsExpectedValues(AdvertisingMode mode, BluetoothLEScanningMode expected)
    {
        Assert.Equal(expected, WindowsAdvertisementHelpers.MapScanningMode(mode));
    }

    [Fact]
    public void MapScanningMode_InvalidMode_Throws()
    {
        var invalid = (AdvertisingMode)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowsAdvertisementHelpers.MapScanningMode(invalid));
    }
}
