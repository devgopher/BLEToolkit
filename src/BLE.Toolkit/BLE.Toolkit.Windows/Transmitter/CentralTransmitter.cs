using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class CentralTransmitter(IOptionsMonitor<TransmitterSettings> settings, DeviceCache deviceCache)
    : BasicBleTransmitter(settings, deviceCache)
{
    private ExpiredList<KeyValuePair<ulong, GattCharacteristicsResult>>? _characteristicsResults = new(settings.CurrentValue.DeviceCache.Timeout,
        () => DateTime.UtcNow);
    

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        StartGattAdvertising();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }

    protected override void InnerTransmit(TransmitElement transmitElement)
    {
        ExecuteWithRetry(() =>
        {
            if (transmitElement.BluetoothAddress == null)
                return;
            WriteToDevice(transmitElement.BluetoothAddress.Value, transmitElement.Data);
        });
    }

    private void WriteToDevice(ulong bluetoothAddress, byte[] data)
    {
        var characteristicsResult = GetCharacteristics(bluetoothAddress);

        var buffer = CreateBuffer(data);
        var writeResult = characteristicsResult.Characteristics[0]
            .WriteValueAsync(buffer, GattWriteOption.WriteWithoutResponse)
            .AsTask()
            .GetAwaiter()
            .GetResult();

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

    private GattCharacteristicsResult GetCharacteristics(ulong bluetoothAddress)
    {
        var cached = _characteristicsResults.FirstOrDefault(cr => cr.Key == bluetoothAddress);
        if (cached.Value != null)
            return cached.Value;
            
        var (serviceUuid, characteristicUuid) = GetPrimaryUuids();

        var device = BluetoothLEDevice
            .FromBluetoothAddressAsync(bluetoothAddress)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (device == null)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException("BLE devices not found");
        }

        var servicesResult = device
            .GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Cached)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (servicesResult == null || servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
        {
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException("GATT services not found");
        }

        var service = servicesResult.Services[0];
        var characteristicsResult = service
            .GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Cached)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (characteristicsResult.Status != GattCommunicationStatus.Success
            || characteristicsResult.Characteristics.Count == 0)
        {
            Console.WriteLine("NF: " + Enum.GetName(characteristicsResult.Status));
            ClearCache(bluetoothAddress);
            throw new InvalidOperationException($"GATT characteristic not found: {characteristicsResult.Status}");
        }

        _characteristicsResults.Add(new KeyValuePair<ulong, GattCharacteristicsResult>(bluetoothAddress, characteristicsResult));
        
        return characteristicsResult;
    }
}