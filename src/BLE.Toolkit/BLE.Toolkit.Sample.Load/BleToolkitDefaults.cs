using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;

namespace BLE.Toolkit.Sample.Load;

internal static class BleToolkitDefaults
{
    public const string ServiceGuid = "0497947e-a031-491b-b1a0-163d605003d5";
    public const string GattCharId = "16b7c725-ac93-4d29-b10e-039042971498";

    public static TransmitterSettings CreateTransmitterSettings() => new()
    {
        ProtocolVersion = ProtocolVersion.BLE5,
        TransmitQueueSize = 10_000,
        RetryPolicy = new RetryPolicySettings
        {
            RetryCount = 3,
            RetryDelay = TimeSpan.FromSeconds(1)
        },
        QueueFilledStrategy = QueueFilledStrategy.DequeueLast,
        DeviceCache = new DeviceCacheSettings { MaxCacheSize = 100 },
        Advertising = new AdvertisingSettings
        {
            Enabled = true,
            Mode = AdvertisingMode.Active
        },
        ServiceSettings = new GattServiceSettings
        {
            Services =
            [
                new GattServiceSetting
                {
                    ServiceUuid = ServiceGuid,
                    Characteristics = new Dictionary<string, string>
                    {
                        { "gattChar", GattCharId }
                    }
                }
            ]
        },
        RateLimiting = new RateLimitingSettings
        {
            Enabled = false,
            RatePeriod = RatePeriod.Second,
            Limit = 1
        }
    };

    public static ReceiverSettings CreateReceiverSettings() => new()
    {
        ProtocolVersion = ProtocolVersion.BLE5,
        ReceiveQueueSize = 10_000,
        QueueFilledStrategy = QueueFilledStrategy.DequeueLast,
        DeviceCache = new DeviceCacheSettings { MaxCacheSize = 100 },
        ServiceSettings = new GattServiceSettings
        {
            Services =
            [
                new GattServiceSetting
                {
                    ServiceUuid = ServiceGuid,
                    Characteristics = new Dictionary<string, string>
                    {
                        { "gattChar", GattCharId }
                    }
                }
            ]
        },
        Advertising = new AdvertisingSettings
        {
            Enabled = true,
            Mode = AdvertisingMode.Passive,
            ManufacturerData =
            [
                new()
                {
                    CompanyId = 0xFFFF,
                    Data = "234"u8.ToArray()
                }
            ]
        }
    };

    public static DeviceCache CreateDeviceCache(TimeSpan? expired = null) => new(expired ?? TimeSpan.FromSeconds(30));
}
