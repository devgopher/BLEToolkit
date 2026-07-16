using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Utils;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Advertisement;

/// <summary>
/// BLE advertisement transmitter for Windows.
/// Publishes advertisements via <see cref="BluetoothLEAdvertisementPublisher"/>.
/// </summary>
/// <remarks>
/// Windows reserves LocalName, Flags and ServiceUuids for system use.
/// Only manufacturer data (0xFF) and non-reserved custom data sections are allowed.
/// See: https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.advertisement.bluetoothleadvertisementpublisher
/// </remarks>
public class WindowsBleAdvertisementTransmitter(
    IOptionsMonitor<AdvertisingSettings> advertisementSettingsMonitor)
    : IAdvertisementTransmitter
{
    /// <summary>
    /// GAP AD types reserved by Windows for BluetoothLEAdvertisementPublisher.
    /// </summary>
    private static readonly HashSet<byte> ReservedDataTypes =
    [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
        0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x14, 0x15, 0x16, 0x17,
        0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0x3D
    ];

    private BluetoothLEAdvertisementPublisher? _advertisementPublisher;

    private IOptionsMonitor<AdvertisingSettings> AdvertisementSettingsMonitor { get; } =
        advertisementSettingsMonitor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        InitAdvertisementPublishing();
        StartAdvertisementPublishing();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_advertisementPublisher?.Status == BluetoothLEAdvertisementPublisherStatus.Started)
            _advertisementPublisher.Stop();

        _advertisementPublisher = null;

        return Task.CompletedTask;
    }

    private void InitAdvertisementPublishing()
    {
        var advertising = AdvertisementSettingsMonitor.CurrentValue;
        if (!advertising.Enabled)
            return;

        // Do not set LocalName, Flags or ServiceUuids — Windows rejects them on Start().
        var publisher = new BluetoothLEAdvertisementPublisher();
        var advertisement = publisher.Advertisement;

        if (advertising.ManufacturerData is { Length: > 0 } manufacturerData)
        {
            advertisement.ManufacturerData.AddRange(manufacturerData.Select(m =>
                new BluetoothLEManufacturerData
                {
                    CompanyId = m.CompanyId,
                    Data = CreateBuffer(m.Data)
                }));
        }

        if (advertising.DataSections is { Length: > 0 } dataSections)
        {
            advertisement.DataSections.AddRange(
                dataSections
                    .Where(d => !ReservedDataTypes.Contains(d.DataType))
                    .Select(d => new BluetoothLEAdvertisementDataSection
                    {
                        DataType = d.DataType,
                        Data = CreateBuffer(d.Data)
                    }));
        }

        var hasManufacturerData = advertisement.ManufacturerData.Count > 0;
        var hasCustomDataSection = advertisement.DataSections.Any(s => !IsManufacturerSpecificSection(s));
        if (!hasManufacturerData && !hasCustomDataSection)
        {
            // A publisher with no custom payload cannot be started.
            throw new InvalidOperationException(
                "BluetoothLEAdvertisementPublisher requires ManufacturerData or a non-reserved DataSection. " +
                "LocalName, Flags and ServiceUuids are not supported by Windows advertisement publisher API.");
        }

        _advertisementPublisher = publisher;
    }

    private void StartAdvertisementPublishing()
    {
        if (_advertisementPublisher?.Status == BluetoothLEAdvertisementPublisherStatus.Created)
            _advertisementPublisher.Start();
    }

    private static IBuffer CreateBuffer(byte[]? data)
    {
        if (data is null || data.Length == 0)
            return new byte[0].AsBuffer();

        return data.AsBuffer();
    }

    private static bool IsManufacturerSpecificSection(BluetoothLEAdvertisementDataSection section) =>
        section.DataType == 0xFF;
}
