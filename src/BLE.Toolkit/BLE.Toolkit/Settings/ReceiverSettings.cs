namespace BLE.Toolkit.Settings;

/// <summary>
///     BLE Receiver settings
/// </summary>
public class ReceiverSettings
{
    public ProtocolVersion ProtocolVersion { get; init; } = ProtocolVersion.BLE4;
    public int ReceiveQueueSize { get; init; } = 1024;
}