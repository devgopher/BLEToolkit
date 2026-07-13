namespace BLE.Toolkit.Advertisement.Models;

/// <summary>
/// User‑defined enumeration that mirrors the BLE advertisement types
/// (the built‑in Windows enum is replaced by this one).
/// </summary>
public enum BleAdvertisementKind : byte
{
    Unknown = 0, // Unknown or unsupported type
    ConnectableUndirected = 0x00, // ADV_IND
    ConnectableDirected = 0x01, // ADV_DIRECT_IND
    ScannableUndirected = 0x02, // ADV_SCAN_IND
    NonConnectableUndirected = 0x03, // ADV_NONCONN_IND
    ScanResponse = 0x04 // SCAN_RSP
}