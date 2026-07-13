namespace BLE.Toolkit.Settings;

/// <summary>
///     BLE advertisement scanning mode.
/// </summary>
public enum AdvertisingMode
{
    /// <summary>
    ///     Passive scanning — does not send scan requests.
    /// </summary>
    Passive,

    /// <summary>
    ///     Active scanning — sends scan requests to scannable advertisements.
    /// </summary>
    Active,

    /// <summary>
    ///     Balanced scanning — alternates between passive and active.
    /// </summary>
    Balanced
}