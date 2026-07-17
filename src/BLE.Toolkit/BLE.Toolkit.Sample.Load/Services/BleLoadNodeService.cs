using System.Text;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Interfaces.Receiver;
using BLE.Toolkit.Sample.Load.Models;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Advertisement;
using BLE.Toolkit.Windows.Receiver;
using BLE.Toolkit.Windows.Transmitter;

namespace BLE.Toolkit.Sample.Load.Services;

public sealed class BleLoadNodeService(
    ReceivedMessageStore messageStore,
    ILogger<BleLoadNodeService> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly DeviceCache _deviceCache = BleToolkitDefaults.CreateDeviceCache();
    private readonly OptionsMock<TransmitterSettings> _transmitterSettings =
        new(BleToolkitDefaults.CreateTransmitterSettings());
    private readonly OptionsMock<ReceiverSettings> _receiverSettings =
        new(BleToolkitDefaults.CreateReceiverSettings());
    private readonly OptionsMock<AdvertisingSettings> _advertisingSettings =
        new(BleToolkitDefaults.CreateReceiverSettings().Advertising);

    private CancellationTokenSource? _cts;
    private Task? _receiverPollTask;

    private WindowsBleAdvertisementReceiver? _advertisementReceiver;
    private CentralTransmitter? _transmitter;
    private WindowsReceiver? _receiver;
    private WindowsBleAdvertisementTransmitter? _advertisementTransmitter;

    private NodeRole _role = NodeRole.None;
    private int _transmitTargetCount;
    private int _transmitEnqueuedCount;
    private string? _lastTransmitMessage;

    public NodeStatusResponse GetStatus()
    {
        return new NodeStatusResponse(
            _role.ToString().ToLowerInvariant(),
            _role != NodeRole.None,
            _role == NodeRole.Transmitter
                ? new TransmitterStatusDto(
                    _transmitTargetCount,
                    _transmitEnqueuedCount,
                    _deviceCache.Count,
                    _lastTransmitMessage)
                : null,
            _role == NodeRole.Receiver
                ? new ReceiverStatusDto(messageStore.TotalCount)
                : null);
    }

    public async Task SetRoleAsync(NodeRole role, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_role == role)
                return;

            await StopCurrentRoleAsync(cancellationToken);

            _role = role;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            switch (role)
            {
                case NodeRole.Transmitter:
                    await StartTransmitterRoleAsync(_cts.Token);
                    break;
                case NodeRole.Receiver:
                    await StartReceiverRoleAsync(_cts.Token);
                    break;
                case NodeRole.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }

            logger.LogInformation("BLE node role set to {Role}", role);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task EnqueueTransmissionAsync(string message, int count, CancellationToken cancellationToken = default)
    {
        if (_role != NodeRole.Transmitter)
            throw new InvalidOperationException("Transmission is only available in transmitter role.");

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message must not be empty.", nameof(message));

        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_transmitter is null)
                throw new InvalidOperationException("Transmitter is not started.");

            var bytes = Encoding.UTF8.GetBytes(message);
            _transmitTargetCount = count;
            _transmitEnqueuedCount = 0;
            _lastTransmitMessage = message;

            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _transmitter.Transmit(bytes);
                _transmitEnqueuedCount++;
            }

            logger.LogInformation("Enqueued {Count} broadcast transmissions", count);
        }
        finally
        {
            _gate.Release();
        }
    }

    public ReceiverMessagesResponse GetMessages(int skip, int take) =>
        messageStore.GetMessages(skip, take);

    public int GetReceivedCount() => messageStore.TotalCount;

    public void ClearReceivedMessages() => messageStore.Clear();

    public async ValueTask DisposeAsync()
    {
        await StopCurrentRoleAsync(CancellationToken.None);
        _gate.Dispose();
    }

    private async Task StartTransmitterRoleAsync(CancellationToken cancellationToken)
    {
        _advertisementReceiver = new WindowsBleAdvertisementReceiver(_advertisingSettings, _deviceCache);
        _transmitter = new CentralTransmitter(_transmitterSettings, _deviceCache);

        _advertisementReceiver.AdvertisementReceived += adv =>
        {
            logger.LogDebug(
                "Advertisement from {Address}, manufacturer sections: {Count}",
                adv.BluetoothAddress,
                adv.ManufacturerData.Count);
        };

        await _advertisementReceiver.StartAsync(cancellationToken);
        await _transmitter.StartAsync(cancellationToken);

        logger.LogInformation("Transmitter role started: scanning and GATT central ready");
    }

    private async Task StartReceiverRoleAsync(CancellationToken cancellationToken)
    {
        messageStore.Clear();
        _receiver = new WindowsReceiver(_receiverSettings);
        _advertisementTransmitter = new WindowsBleAdvertisementTransmitter(_advertisingSettings);

        await _receiver.StartAsync(cancellationToken);
        await _advertisementTransmitter.StartAsync(cancellationToken);

        _receiverPollTask = PollReceiverAsync(_receiver, cancellationToken);
        logger.LogInformation("Receiver role started: GATT server and advertisement publisher ready");
    }

    private async Task PollReceiverAsync(IReceiver receiver, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            while (receiver.TryGetLast(out var data))
                messageStore.Add(data);

            try
            {
                await Task.Delay(50, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task StopCurrentRoleAsync(CancellationToken cancellationToken)
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        if (_receiverPollTask is not null)
        {
            try
            {
                await _receiverPollTask;
            }
            catch (OperationCanceledException)
            {
            }

            _receiverPollTask = null;
        }

        if (_advertisementReceiver is not null)
        {
            await _advertisementReceiver.StopAsync(cancellationToken);
            _advertisementReceiver = null;
        }

        if (_transmitter is not null)
        {
            await _transmitter.StopAsync(cancellationToken);
            _transmitter = null;
        }

        if (_receiver is not null)
        {
            await _receiver.StopAsync(cancellationToken);
            _receiver = null;
        }

        if (_advertisementTransmitter is not null)
        {
            await _advertisementTransmitter.StopAsync(cancellationToken);
            _advertisementTransmitter = null;
        }

        _transmitTargetCount = 0;
        _transmitEnqueuedCount = 0;
        _lastTransmitMessage = null;
        _role = NodeRole.None;
    }
}
