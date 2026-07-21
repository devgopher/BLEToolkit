using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class CentralTransmitter(IOptionsMonitor<TransmitterSettings> settings, DeviceCache deviceCache)
    : BasicBleTransmitter(settings, deviceCache)
{
    private readonly ExpiredList<KeyValuePair<ulong, GattCharacteristicsResult>>? _characteristicsResults = new(settings.CurrentValue.DeviceCache.Timeout,
        () => DateTime.UtcNow);
    

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
        var toDelete = _characteristicsResults.Where(c => c.Key == bluetoothAddress);
        foreach (KeyValuePair<ulong, GattCharacteristicsResult> characteristic in toDelete)
        {
            _characteristicsResults.Remove(characteristic);
        }
    }

    private async Task<GattCharacteristicsResult> GetCharacteristicsAsync(ulong bluetoothAddress)
    {
        var cached = _characteristicsResults.FirstOrDefault(cr => cr.Key == bluetoothAddress);
        if (cached.Value != null)
            return cached.Value;
            
        var (serviceUuid, characteristicUuid) = GetPrimaryUuids();

        var device = await BluetoothLEDevice
            .FromBluetoothAddressAsync(bluetoothAddress)
            .AsTask();

        if (device == null)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException("BLE devices not found");
        }

        var servicesResult = await device
            .GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Cached)
            .AsTask();

        if (servicesResult == null || servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException("GATT services not found");
        }

        var service = servicesResult.Services[0];
        var characteristicsResult = await service
            .GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Cached)
            .AsTask();

        if (characteristicsResult.Status != GattCommunicationStatus.Success
            || characteristicsResult.Characteristics.Count == 0)
        {
            Console.WriteLine($"NF: {Enum.GetName(characteristicsResult.Status)}");
            ClearCache(bluetoothAddress);
            
             if (characteristicsResult.Status == GattCommunicationStatus.AccessDenied)
                 await Task.Delay(500);
            
            throw new InvalidOperationException($"GATT characteristic not found: {characteristicsResult.Status}");
        }

        _characteristicsResults.Add(new KeyValuePair<ulong, GattCharacteristicsResult>(bluetoothAddress, characteristicsResult));
        
        return characteristicsResult;
    }
}
