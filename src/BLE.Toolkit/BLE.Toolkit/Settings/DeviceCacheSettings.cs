namespace BLE.Toolkit.Settings;

/// <summary>
/// Local device cache settings 
/// </summary>
public class DeviceCacheSettings
{
    /// <summary>
    /// Maximum cache size
    /// </summary>
    public int MaxCacheSize { get; set; }

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 30;
}