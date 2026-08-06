using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Tests;

internal static class BleAdvertisementFactory
{
    private static readonly BleAdvertisement Shared = new();

    /// <summary>
    /// Returns the shared advertisement instance configured with the given values.
    /// Does not allocate a new <see cref="BleAdvertisement"/> on each call.
    /// </summary>
    public static BleAdvertisement Create(
        ulong bluetoothAddress = 0xAABBCCDDEEFF,
        short rssi = -60,
        BleAdvertisementKind kind = BleAdvertisementKind.ConnectableUndirected,
        string? localName = "device",
        IEnumerable<Guid>? serviceUuids = null,
        IEnumerable<BleAdvertisement.ManufacturerRecord>? manufacturerData = null)
    {
        Shared.BluetoothAddress = bluetoothAddress;
        Shared.Rssi = rssi;
        Shared.Kind = kind;
        Shared.LocalName = localName;

        Shared.ServiceUuids.Clear();
        if (serviceUuids != null)
        {
            foreach (var uuid in serviceUuids)
                Shared.ServiceUuids.Add(uuid);
        }

        Shared.ManufacturerData.Clear();
        if (manufacturerData != null)
        {
            foreach (var record in manufacturerData)
                Shared.ManufacturerData.Add(record);
        }

        return Shared;
    }
}
