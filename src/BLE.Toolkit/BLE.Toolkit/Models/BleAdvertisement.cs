namespace BLE.Toolkit.Advertisement.Models;

/// <summary>
/// Model class that represents a single BLE advertisement
/// </summary>
public class BleAdvertisement
{
    /// <summary>Bluetooth MAC address (64‑bit, no separators).</summary>
    public ulong BluetoothAddress { get; set; }

    /// <summary>Signal strength in dBm.</summary>
    public short Rssi { get; set; }

    /// <summary>Type of the advertisement packet – using our own enum.</summary>
    public BleAdvertisementKind Kind { get; set; }

    /// <summary>Device’s local name (may be empty).</summary>
    public string? LocalName { get; set; } = string.Empty;

    /// <summary>List of advertised service UUIDs.</summary>
    public List<Guid> ServiceUuids { get; set; } = new();

    /// <summary>Manufacturer‑specific data blocks.</summary>
    public List<ManufacturerRecord> ManufacturerData { get; set; } = new();

    /// <summary>
    /// Nested class that stores a single manufacturer data record.
    /// </summary>
    public class ManufacturerRecord
    {
        /// <summary>Company identifier (16‑bit, assigned by Bluetooth SIG).</summary>
        public ushort CompanyId { get; set; }

        /// <summary>Raw payload bytes supplied by the manufacturer.</summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}