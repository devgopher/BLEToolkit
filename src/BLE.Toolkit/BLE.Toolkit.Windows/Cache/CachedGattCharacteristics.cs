using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE.Toolkit.Windows.Cache;

/// <summary>
/// Cached GATT characteristics result for a Bluetooth address.
/// </summary>
public sealed class CachedGattCharacteristics(ulong bluetoothAddress, GattCharacteristicsResult characteristicsResult)
{
    public ulong BluetoothAddress { get; } = bluetoothAddress;

    public GattCharacteristicsResult CharacteristicsResult { get; } = characteristicsResult;
}
