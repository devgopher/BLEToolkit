/// <summary>
/// Configuration root for defining which GATT services the application should interact with.
/// </summary>
public class GattServiceSettings
{
    /// <summary>
    /// Collection of GATT services to interact with.
    /// </summary>
    public GattServiceSetting[] Services { get; init; } = Array.Empty<GattServiceSetting>();
}