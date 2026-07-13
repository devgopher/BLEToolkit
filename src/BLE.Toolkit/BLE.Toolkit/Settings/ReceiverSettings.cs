namespace BLE.Toolkit.Settings;

/// <summary>
///     BLE Receiver settings
/// </summary>
public class ReceiverSettings
{
    public ProtocolVersion ProtocolVersion { get; init; } = ProtocolVersion.BLE4;
    public int ReceiveQueueSize { get; init; } = 1024;
    public QueueFilledStrategy QueueFilledStrategy { get; init; }
    public required GattServiceSettings ServiceSettings { get; init; } = new();
    
    public required DeviceCacheSettings DeviceCache { get; init; }
}