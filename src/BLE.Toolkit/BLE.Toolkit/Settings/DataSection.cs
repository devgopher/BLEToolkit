namespace BLE.Toolkit.Settings;

/// <summary>
/// Data section
/// </summary>
public record DataSection
{
    public byte[]? Data { get; init; }
    public byte DataType { get; init; }
}