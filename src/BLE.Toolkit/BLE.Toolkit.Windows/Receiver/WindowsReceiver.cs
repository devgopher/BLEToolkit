using System.Collections.Concurrent;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using BLE.Toolkit.Receiver;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Receiver;

/// <summary>
///     Windows BLE receiver: local GATT server with writable characteristics from
///     <see cref="ReceiverSettings.ServiceSettings" />; incoming writes are surfaced via <see cref="GetDataChunk" />.
/// </summary>
public class WindowsReceiver(IOptionsMonitor<ReceiverSettings> settings) : BasicReceiver(settings)
{
    private readonly List<GattLocalCharacteristic> _receiveCharacteristics = [];
    private readonly BlockingCollection<byte[]> _pendingChunks = new();

    private GattServiceProvider? _bleServiceProvider;
    private CancellationToken _cancellationToken;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        await InitializeGattServerAsync(cancellationToken).ConfigureAwait(false);
        StartGattAdvertising();
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var characteristic in _receiveCharacteristics)
            characteristic.WriteRequested -= OnCharacteristicWriteRequested;
        _receiveCharacteristics.Clear();

        _bleServiceProvider?.StopAdvertising();
        _bleServiceProvider = null;
        _pendingChunks.CompleteAdding();

        return Task.CompletedTask;
    }

    protected override byte[] GetDataChunk()
    {
        try
        {
            return _pendingChunks.Take(_cancellationToken);
        }
        catch (InvalidOperationException) when (_pendingChunks.IsCompleted)
        {
            throw new OperationCanceledException(_cancellationToken);
        }
    }

    private async Task InitializeGattServerAsync(CancellationToken cancellationToken)
    {
        var serviceConfigs = settings.CurrentValue.ServiceSettings.Services;
        if (serviceConfigs.Length == 0)
            throw new InvalidOperationException("At least one GATT service must be configured in ServiceSettings.");

        var serviceSetting = serviceConfigs[0];
        if (serviceSetting.Characteristics.Count == 0)
            throw new InvalidOperationException("At least one GATT characteristic must be configured.");

        var create = await GattServiceProvider.CreateAsync(Guid.Parse(serviceSetting.ServiceUuid))
            .AsTask(cancellationToken)
            .ConfigureAwait(false);
        if (create.Error != BluetoothError.Success || create.ServiceProvider == null)
            throw new InvalidOperationException($"Failed to create GATT service: {create.Error}");

        _bleServiceProvider = create.ServiceProvider;

        foreach (var (_, characteristicUuid) in serviceSetting.Characteristics)
        {
            var parameters = new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Write
                                           | GattCharacteristicProperties.WriteWithoutResponse
                                           | GattCharacteristicProperties.Read,
                WriteProtectionLevel = GattProtectionLevel.Plain,
                ReadProtectionLevel = GattProtectionLevel.Plain
            };

            var result = await _bleServiceProvider.Service
                .CreateCharacteristicAsync(Guid.Parse(characteristicUuid), parameters)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);

            if (result.Error != BluetoothError.Success || result.Characteristic == null)
                throw new InvalidOperationException($"Failed to create characteristic: {result.Error}");

            result.Characteristic.WriteRequested += OnCharacteristicWriteRequested;
            _receiveCharacteristics.Add(result.Characteristic);
        }
    }

    private void StartGattAdvertising()
    {
        if (_bleServiceProvider == null)
            return;

        _bleServiceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters
        {
            IsConnectable = true,
            IsDiscoverable = true
        });
    }

    private async void OnCharacteristicWriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
    {
        try
        {
            using var deferral = args.GetDeferral();
            var req = await args.GetRequestAsync().AsTask(_cancellationToken).ConfigureAwait(false);
            if (req?.Value == null)
                return;

            var reader = DataReader.FromBuffer(req.Value);
            var length = reader.UnconsumedBufferLength;
            if (length == 0)
                return;

            var data = new byte[length];
            reader.ReadBytes(data);

            if (req.Option == GattWriteOption.WriteWithResponse)
                req.Respond();

            if (!_pendingChunks.IsAddingCompleted)
                _pendingChunks.Add(data, _cancellationToken);
        }
        catch
        {
            // ignore malformed writes
        }
    }
}
