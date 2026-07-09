/// <summary>
/// Defines a single GATT service and its characteristics configuration.
/// </summary>
public class GattServiceSetting
{
    /// <summary>
    /// UUID (as a string) of the GATT service.
    /// </summary>
    public required string ServiceUuid { get; init; }
    
    /// <summary>
    /// Characteristics dictionary keyed by a logical name.
    /// Value is the characteristic UUID (as a string).
    /// </summary>
    public required Dictionary<string, string> Characteristics { get; init; }
}