using Windows.Devices.Bluetooth.Advertisement;
using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Advertisement;

/// <summary>
/// BLE advertisement transmitter for Windows.
/// Publishes advertisements via <see cref="BluetoothLEAdvertisementPublisher"/>.
/// </summary>
public class WindowsBleAdvertisementTransmitter(
    IOptionsMonitor<AdvertisingSettings> advertisementSettingsMonitor)
    : IAdvertisementTransmitter
{
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

        var advertisement = new BluetoothLEAdvertisement();

        if (!string.IsNullOrWhiteSpace(advertising.LocalName))
            advertisement.LocalName = advertising.LocalName;

        foreach (var serviceUuid in advertising.ServiceUuids)
            advertisement.ServiceUuids.Add(serviceUuid);

        _advertisementPublisher = new BluetoothLEAdvertisementPublisher(advertisement);
    }

    private void StartAdvertisementPublishing()
    {
        if (_advertisementPublisher?.Status == BluetoothLEAdvertisementPublisherStatus.Created)
            _advertisementPublisher.Start();
    }
}
