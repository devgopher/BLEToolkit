using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class CentralTransmitter(IOptionsMonitor<TransmitterSettings> settings, DeviceCache deviceCache)
    : BasicBleTransmitter(settings, deviceCache)
{
    private readonly Lock _connectionLock = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        StartGattAdvertising();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_connectionLock)
        {
        }

        return base.StopAsync(cancellationToken);
    }

    protected override void InnerTransmit(TransmitElement transmitElement)
    {
        ExecuteWithRetry(() =>
        {
            if (transmitElement?.BluetoothAddress == null)
                return;

            WriteToDevice(transmitElement.BluetoothAddress.Value, transmitElement.Data);
        });
    }

    private void WriteToDevice(ulong bluetoothAddress, byte[] data)
    {
        var (serviceUuid, characteristicUuid) = GetPrimaryUuids();

        using var device = BluetoothLEDevice
            .FromBluetoothAddressAsync(bluetoothAddress)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (device == null)
            return;
        
        var servicesResult = device
            .GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Uncached)
            .AsTask()
            .GetAwaiter()
            .GetResult();
        
        if (servicesResult == null)
            return;

        if (servicesResult.Status != GattCommunicationStatus.Success || servicesResult.Services.Count == 0)
            throw new InvalidOperationException($"GATT service not found: {servicesResult.Status}");

        var service = servicesResult.Services[0];
        var characteristicsResult = service
            .GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Uncached)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (characteristicsResult.Status != GattCommunicationStatus.Success
            || characteristicsResult.Characteristics.Count == 0)
            throw new InvalidOperationException($"GATT characteristic not found: {characteristicsResult.Status}");

        var buffer = CreateBuffer(data);
        var writeResult = characteristicsResult.Characteristics[0]
            .WriteValueAsync(buffer, GattWriteOption.WriteWithResponse)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        if (writeResult != GattCommunicationStatus.Success)
            throw new InvalidOperationException($"GATT write failed: {writeResult}");
    }
}