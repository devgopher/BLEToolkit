namespace BLE.Toolkit.Settings;

/// <summary>
/// Manufacturer data
/// </summary>
public record ManufacturerData
{
    public ushort CompanyId{ get; init; }
    public byte[]? Data { get; init; }
}