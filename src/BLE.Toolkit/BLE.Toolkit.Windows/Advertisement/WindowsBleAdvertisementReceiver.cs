using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;
using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Advertisement.Models;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Advertisement;

/// <summary>
/// BLE advertisement receiver for Windows
/// </summary>
/// <param name="advertisementSettingsMonitor"></param>
/// <param name="deviceCache"></param>
public class WindowsBleAdvertisementReceiver(
    IOptionsMonitor<AdvertisingSettings> advertisementSettingsMonitor,
    DeviceCache deviceCache)
    : BasicAdvertisementReceiver
{
    private BluetoothLEAdvertisementWatcher? _advertisementWatcher;

    private IOptionsMonitor<AdvertisingSettings> AdvertisementSettingsMonitor { get; } = advertisementSettingsMonitor;

    private static BluetoothLEScanningMode MapScanningMode(AdvertisingMode mode)
    {
        return mode switch
        {
            AdvertisingMode.Passive => BluetoothLEScanningMode.Passive,
            AdvertisingMode.Active or AdvertisingMode.Balanced => BluetoothLEScanningMode.Active,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }


    public override Task StartAsync(CancellationToken cancellationToken)
    {
        InitAdvertisementScanning();

        if (_advertisementWatcher != null)
            _advertisementWatcher.Received += OnAdvertisementReceived;

        StartAdvertisementScanning();

        return Task.CompletedTask;
    }

    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender,
        BluetoothLEAdvertisementReceivedEventArgs args)
    {
        if (deviceCache.Any(d => d.BluetoothAddress.Equals(args.BluetoothAddress)))
            return;

        var advertisement = new BleAdvertisement
        {
            BluetoothAddress = args.BluetoothAddress,
            // Rssi= args.Advertisement.ManufacturerData,
            Kind = BleAdvertisementKind.Unknown,
            LocalName = args.Advertisement.LocalName,
            ServiceUuids = args.Advertisement.ServiceUuids.ToList(),
            ManufacturerData = args.Advertisement.ManufacturerData.Select(m => new BleAdvertisement.ManufacturerRecord
            {
                CompanyId = m.CompanyId,
                Data = m.Data.ToArray()
            }).ToList()
        };
        
        deviceCache.Add(advertisement);

        base.OnAdvertisementReceived(advertisement);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _advertisementWatcher?.Stop();
        
        return Task.CompletedTask;
    }
    
    private void InitAdvertisementScanning()
    {
        var advertising = AdvertisementSettingsMonitor.CurrentValue;
        if (!advertising.Enabled)
            return;

        _advertisementWatcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = MapScanningMode(advertising.Mode)
        };
    }

    private void StartAdvertisementScanning() => _advertisementWatcher?.Start();
}