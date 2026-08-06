using Windows.Devices.Bluetooth.Advertisement;
using BLE.Toolkit.Settings;

namespace BLE.Toolkit.Windows.Advertisement;

/// <summary>
/// Pure helpers for Windows BLE advertisement publish/scan rules.
/// </summary>
internal static class WindowsAdvertisementHelpers
{
    /// <summary>
    /// GAP AD types reserved by Windows for <see cref="BluetoothLEAdvertisementPublisher"/>.
    /// </summary>
    private static readonly HashSet<byte> ReservedDataTypes =
    [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
        0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x14, 0x15, 0x16, 0x17,
        0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x3D
    ];

    internal static bool IsReservedDataType(byte dataType) => ReservedDataTypes.Contains(dataType);

    internal static bool IsManufacturerSpecificDataType(byte dataType) => dataType == 0xFF;

    internal static IEnumerable<DataSection> FilterPublishableDataSections(IEnumerable<DataSection> sections) =>
        sections.Where(d => !IsReservedDataType(d.DataType));

    /// <summary>
    /// Publisher needs ManufacturerData or a non-reserved, non-0xFF data section.
    /// </summary>
    internal static bool HasValidPublisherPayload(
        bool hasManufacturerData,
        IEnumerable<byte> dataSectionTypes)
    {
        if (hasManufacturerData)
            return true;

        return dataSectionTypes.Any(t => !IsManufacturerSpecificDataType(t));
    }

    internal static BluetoothLEScanningMode MapScanningMode(AdvertisingMode mode) =>
        mode switch
        {
            AdvertisingMode.Passive => BluetoothLEScanningMode.Passive,
            AdvertisingMode.Active or AdvertisingMode.Balanced => BluetoothLEScanningMode.Active,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
}
