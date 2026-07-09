using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Transmitter;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace BLE.Toolkit.Windows.Transmitter;

public abstract class BasicBleTransmitter(IOptionsMonitor<TransmitterSettings> settings) : BasicTransmitter(settings)
{
    private IOptionsMonitor<TransmitterSettings> TransmitterSettingsMonitor { get; } = settings;

    protected BluetoothLEAdvertisementWatcher? AdvertisementWatcher { get; private set; }
    private BluetoothLEAdvertisementPublisher? AdvertisementPublisher { get; set; }
    private GattServiceProvider? BleServiceProvider { get; set; }
    protected GattLocalCharacteristic? LocalTransmitCharacteristic { get; private set; }

    private static BluetoothLEScanningMode MapScanningMode(AdvertisingMode mode) => mode switch
    {
        AdvertisingMode.Passive => BluetoothLEScanningMode.Passive,
        AdvertisingMode.Active or AdvertisingMode.Balanced => BluetoothLEScanningMode.Active,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    protected void StartAdvertisementScanning()
    {
        var advertising = TransmitterSettingsMonitor.CurrentValue.Advertising;
        if (!advertising.Enabled)
            return;

        AdvertisementWatcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = MapScanningMode(advertising.Mode)
        };
        AdvertisementWatcher.Start();
    }

    private void EnsureAdvertisementPublisher()
    {
        if (AdvertisementPublisher != null)
            return;

        AdvertisementPublisher = new BluetoothLEAdvertisementPublisher();

        var serviceSetting = GetPrimaryServiceSetting();
        if (serviceSetting != null)
            AdvertisementPublisher.Advertisement.ServiceUuids.Add(Guid.Parse(serviceSetting.ServiceUuid));
    }

    protected void StartAdvertisementPublishing()
    {
        var advertising = TransmitterSettingsMonitor.CurrentValue.Advertising;
        if (!advertising.Enabled)
            return;

        EnsureAdvertisementPublisher();

        if (AdvertisementPublisher!.Status == BluetoothLEAdvertisementPublisherStatus.Created)
            AdvertisementPublisher.Start();
    }
    
    protected async Task InitializeGattServerAsync(CancellationToken cancellationToken)
    {
        var serviceSetting = GetPrimaryServiceSetting()
                             ?? throw new InvalidOperationException("At least one GATT service must be configured.");

        var serviceUuid = Guid.Parse(serviceSetting.ServiceUuid);
        var result = await GattServiceProvider.CreateAsync(serviceUuid).AsTask(cancellationToken);
        if (result.Error != BluetoothError.Success)
            throw new InvalidOperationException($"Failed to create GATT service: {result.Error}");

        BleServiceProvider = result.ServiceProvider;
        LocalTransmitCharacteristic = await CreateNotifyCharacteristicAsync(
            BleServiceProvider,
            serviceSetting,
            cancellationToken);
    }

    protected void StartGattAdvertising()
    {
        if (BleServiceProvider == null || !TransmitterSettingsMonitor.CurrentValue.Advertising.Enabled)
            return;

        var parameters = new GattServiceProviderAdvertisingParameters
        {
            IsConnectable = true,
            IsDiscoverable = true
        };

        BleServiceProvider.StartAdvertising(parameters);
    }

    private GattServiceSetting? GetPrimaryServiceSetting() =>
        TransmitterSettingsMonitor.CurrentValue.ServiceSettings.Services.FirstOrDefault();

    protected (Guid ServiceUuid, Guid CharacteristicUuid) GetPrimaryUuids()
    {
        var serviceSetting = GetPrimaryServiceSetting()
                             ?? throw new InvalidOperationException("At least one GATT service must be configured.");

        var (_, characteristicUuid) = serviceSetting.Characteristics.First();
        return (Guid.Parse(serviceSetting.ServiceUuid), Guid.Parse(characteristicUuid));
    }

    protected void ExecuteWithRetry(Action action)
    {
        var retry = TransmitterSettingsMonitor.CurrentValue.RetryPolicy;

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retry.RetryCount,
                Delay = retry.RetryDelay,
                BackoffType = DelayBackoffType.Constant
            })
            .Build();

        pipeline.Execute(action);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        StopAdvertisementScanning();
        StopAdvertisementPublishing();
        StopGattServer(cancellationToken);
        return base.StopAsync(cancellationToken);
    }

    private void StopAdvertisementScanning()
    {
        AdvertisementWatcher?.Stop();
        AdvertisementWatcher = null;
    }

    private void StopAdvertisementPublishing()
    {
        if (AdvertisementPublisher?.Status == BluetoothLEAdvertisementPublisherStatus.Started)
            AdvertisementPublisher.Stop();

        AdvertisementPublisher = null;
    }

    private void StopGattServer(CancellationToken cancellationToken)
    {
        if (BleServiceProvider != null)
        {
            BleServiceProvider.StopAdvertising();
            BleServiceProvider = null;
        }

        LocalTransmitCharacteristic = null;
    }

    private static async Task<GattLocalCharacteristic> CreateNotifyCharacteristicAsync(
        GattServiceProvider provider,
        GattServiceSetting serviceSetting,
        CancellationToken cancellationToken)
    {
        var (_, characteristicUuid) = serviceSetting.Characteristics.First();
        var parameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
            ReadProtectionLevel = GattProtectionLevel.Plain,
            WriteProtectionLevel = GattProtectionLevel.Plain
        };

        var result = await provider.Service
            .CreateCharacteristicAsync(Guid.Parse(characteristicUuid), parameters)
            .AsTask(cancellationToken);

        return result.Error != BluetoothError.Success ? throw new InvalidOperationException($"Failed to create characteristic: {result.Error}") : result.Characteristic;
    }
}
