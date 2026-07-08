namespace BLE.Toolkit.Settings;

/// <summary>
/// BLE Transmitter settings
/// </summary>
public class TransmitterSettings
{
    public ProtocolVersion ProtocolVersion { get; init; } = ProtocolVersion.BLE4;
    public int TransmitQueueSize { get; init; } = 1024; 
    public RetryPolicySettings RetryPolicy { get; init; } = new();
}