namespace BLE.Toolkit.Settings;

/// <summary>
///     BLE advertising settings.
/// </summary>
public class AdvertisingSettings
{
    /// <summary>
    ///     Whether BLE advertising/scanning is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    ///     Advertisement scanning mode.
    /// </summary>
    public AdvertisingMode Mode { get; init; } = AdvertisingMode.Balanced;
}
