using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE.Toolkit.Windows.Cache;

/// <summary>
/// Cached GATT services result for a Bluetooth address.
/// </summary>
public sealed class CachedGattServices(ulong bluetoothAddress, GattDeviceServicesResult servicesResult)
{
    public ulong BluetoothAddress { get; } = bluetoothAddress;

    public GattDeviceServicesResult ServicesResult { get; } = servicesResult;
}
