namespace BLE.Toolkit.Settings;

/// <summary>
/// Retry policy settings
/// </summary>
public class RetryPolicySettings
{
    public int RetryCount { get; init; } = 3;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(50);
}