using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BLE.Toolkit.Receiver;
using BLE.Toolkit.Settings;
using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Windows.Receiver;

public class Receiver(IOptionsMonitor<ReceiverSettings> settings) : BasicReceiver(settings)
{
    private BluetoothLEAdvertisementWatcher? _advertisementWatcher;
    private GattLocalCharacteristic[]? _bleCharacteristics;
    private GattServiceProvider? _bleServiceProvider;

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override byte[] GetDataChunk()
    {
        throw new NotImplementedException();
    }
}