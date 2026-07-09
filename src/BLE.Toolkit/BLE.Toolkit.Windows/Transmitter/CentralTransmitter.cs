using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public class CentralTransmitter(IOptionsMonitor<TransmitterSettings> settings) : BasicBleTransmitter(settings)
{
    private readonly Lock _connectionLock = new();
    private ulong? _targetBluetoothAddress;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        StartAdvertisementScanning();

        if (AdvertisementWatcher != null)
            AdvertisementWatcher.Received += OnAdvertisementReceived;

        StartAdvertisementPublishing();
        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (AdvertisementWatcher != null)
            AdvertisementWatcher.Received -= OnAdvertisementReceived;

        lock (_connectionLock)
            _targetBluetoothAddress = null;

        return base.StopAsync(cancellationToken);
    }

    protected override void InnerTransmit(byte[] data)
    {
        var bluetoothAddress = GetTargetBluetoothAddress();
        if (bluetoothAddress == null)
            return;

        ExecuteWithRetry(() => WriteToDevice(bluetoothAddress.Value, data));
    }

    private void OnAdvertisementReceived(
        BluetoothLEAdvertisementWatcher sender,
        BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var (serviceUuid, _) = GetPrimaryUuids();
        if (!args.Advertisement.ServiceUuids.Contains(serviceUuid))
            return;

        lock (_connectionLock)
            _targetBluetoothAddress = args.BluetoothAddress;
    }

    private void WriteToDevice(ulong bluetoothAddress, byte[] data)
    {
        var (serviceUuid, characteristicUuid) = GetPrimaryUuids();

        using var device = BluetoothLEDevice
            .FromBluetoothAddressAsync(bluetoothAddress)
            .AsTask()
            .GetAwaiter()
            .GetResult();

        var servicesResult = device
            .GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Uncached)
            .AsTask()
            .GetAwaiter()
            .GetResult();

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

    private ulong? GetTargetBluetoothAddress()
    {
        lock (_connectionLock)
            return _targetBluetoothAddress;
    }

    private static IBuffer CreateBuffer(byte[] data)
    {
        var writer = new DataWriter();
        writer.WriteBytes(data);
        return writer.DetachBuffer();
    }
}
