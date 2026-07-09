using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Transmitter;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Transmitter;

public abstract class BasicBleTransmitter : BasicTransmitter
{
    protected readonly BluetoothLEAdvertisementWatcher? _advertisementWatcher;
    protected readonly GattLocalCharacteristic[]? _bleCharacteristics;
    protected readonly GattServiceProvider? _bleServiceProvider;

    protected BasicBleTransmitter(IOptionsMonitor<TransmitterSettings> settings) : base(settings)
    {
        
    }
}