using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Cache;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class CentralTransmitter(IOptionsMonitor<TransmitterSettings> settings, DeviceCache deviceCache)
    : BasicBleTransmitter(settings, deviceCache)
{
    private readonly GattServicesCache _gattServicesCache = new();
    private readonly GattCharacteristicsCache _gattCharacteristicsCache = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        StartGattAdvertising();
        return base.StartAsync(cancellationToken);
    }

    protected override Task<bool> InnerTransmitAsync(TransmitElement transmitElement)
    {
        return ExecuteWithRetryAsync(async _ =>
        {
            if (transmitElement.BluetoothAddress == null)
                return;
            await WriteToDeviceAsync(transmitElement.BluetoothAddress.Value, transmitElement.Data);
        });
    }

    private async Task WriteToDeviceAsync(ulong bluetoothAddress, byte[] data)
    {
        var characteristicsResult = await GetCharacteristicsAsync(bluetoothAddress);

        var buffer = CreateBuffer(data);
        var writeResult = await characteristicsResult.Characteristics[0]
            .WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse)
            .AsTask();

        if (writeResult != GattCommunicationStatus.Success)
        {
            ClearCache(bluetoothAddress);

            throw new InvalidOperationException($"GATT write failed: {writeResult}");
        }
    }

    private void ClearCache(ulong bluetoothAddress)
    {
        var services = _gattServicesCache.FindByAddress(bluetoothAddress);
        if (services is not null)
            _gattServicesCache.Remove(services);
        
        var characteristics = _gattCharacteristicsCache.FindByAddress(bluetoothAddress);
        if (characteristics is not null)
            _gattCharacteristicsCache.Remove(characteristics);
    }

    private async Task<GattCharacteristicsResult> GetCharacteristicsAsync(ulong bluetoothAddress)
    {
        var cachedCharacteristics = _gattCharacteristicsCache.FindByAddress(bluetoothAddress);
        if (cachedCharacteristics is not null)
            return cachedCharacteristics.CharacteristicsResult;

        var (serviceUuid, characteristicUuid) = GetPrimaryUuids();

        var device = await BluetoothLEDevice
            .FromBluetoothAddressAsync(bluetoothAddress)
            .AsTask();

        if (device == null)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException($"BLE devices not found {bluetoothAddress}");
        }

        var service = await GetServiceAsync(device, bluetoothAddress, serviceUuid);
        var characteristicsResult = await service
            .GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Uncached)
            .AsTask();

        if (characteristicsResult.Status != GattCommunicationStatus.Success
            || characteristicsResult.Characteristics.Count == 0)
        {
            Console.WriteLine($"NF: {Enum.GetName(characteristicsResult.Status)}");
            ClearCache(bluetoothAddress);

            if (characteristicsResult.Status == GattCommunicationStatus.AccessDenied)
                await Task.Delay(500);

            throw new InvalidOperationException($"GATT characteristic not found: {characteristicsResult.Status} {bluetoothAddress}");
        }

        _gattCharacteristicsCache.Add(new CachedGattCharacteristics(bluetoothAddress, characteristicsResult));

        return characteristicsResult;
    }

    private async Task<GattDeviceService> GetServiceAsync(
        BluetoothLEDevice device,
        ulong bluetoothAddress,
        Guid serviceUuid)
    {
        var cachedServices = _gattServicesCache.FindByAddress(bluetoothAddress);
        if (cachedServices is not null)
            return cachedServices.ServicesResult.Services[0];

        var servicesResult = await device
            .GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Uncached)
            .AsTask();

        if (servicesResult == null || servicesResult.Status != GattCommunicationStatus.Success ||
            servicesResult.Services.Count == 0)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException("GATT services not found");
        }

        _gattServicesCache.Add(new CachedGattServices(bluetoothAddress, servicesResult));

        return servicesResult.Services[0];
    }
}
