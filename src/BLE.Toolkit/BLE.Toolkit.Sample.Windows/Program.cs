using System.Text;
using BLE.Toolkit;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Advertisement;
using BLE.Toolkit.Windows.Receiver;
using BLE.Toolkit.Windows.Transmitter;

var serviceGuid = "0497947e-a031-491b-b1a0-163d605003d5";
var gattCharId = "16b7c725-ac93-4d29-b10e-039042971498";
var deviceCache = new DeviceCache(TimeSpan.FromSeconds(30));
var transmitterSettings = new OptionsMock<TransmitterSettings>(new TransmitterSettings
{
    ProtocolVersion = ProtocolVersion.BLE5,
    TransmitQueueSize = 10,
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
                ServiceUuid = serviceGuid,
                Characteristics = new Dictionary<string, string>
                {
                    { "gattChar", gattCharId }
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
});

var receiverSettings = new OptionsMock<ReceiverSettings>(new ReceiverSettings
{
    ProtocolVersion = ProtocolVersion.BLE5,
    ReceiveQueueSize = 1,
    QueueFilledStrategy = QueueFilledStrategy.DequeueLast,
    DeviceCache = new DeviceCacheSettings
    {
        MaxCacheSize = 100
    },
    ServiceSettings = new GattServiceSettings
    {
        Services =
        [
            new GattServiceSetting
            {
                ServiceUuid = serviceGuid,
                Characteristics = new Dictionary<string, string>
                {
                    {
                        "gattChar", gattCharId
                    }
                }
            }
        ]
    },
    Advertising = new AdvertisingSettings
    {
        Enabled =  true,
        Mode = AdvertisingMode.Passive
    }
});

var advSettings = new OptionsMock<AdvertisingSettings>(receiverSettings.CurrentValue.Advertising);
var advTransmitterSettings = new OptionsMock<AdvertisingSettings>(transmitterSettings.CurrentValue.Advertising);


Console.WriteLine("Choose your role: 0 - transmitter, 1 - receiver");

var role = Console.ReadLine();
CancellationTokenSource cts = new();

while (role != "0" && role != "1")
{
    Console.WriteLine("Choose your role: 0 - transmitter, 1 - receiver");
    role = Console.ReadLine();
}

if (role == "0")
{
    Console.WriteLine("You are running on 0 - transmitter. Please, enter a short message:");
    var message = Console.ReadLine();

    while (string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine("You are running on 0 - transmitter. Please, enter a short message:");
        message = Console.ReadLine();
    }

    var bytes = Encoding.UTF8.GetBytes(message);

    var advertisementReceiver = new WindowsBleAdvertisementReceiver(advSettings, deviceCache);
    
    Console.WriteLine("Starting receiving advertisements...");
    advertisementReceiver.StartAsync(cts.Token).Wait();


    advertisementReceiver.AdvertisementReceived += (adv) =>
    {
        Console.WriteLine($"ADV: {adv.LocalName} {adv.BluetoothAddress}");
    };
    
    var transmitter = new CentralTransmitter(transmitterSettings, deviceCache);

        
    Console.WriteLine("Starting transmission...");
    var startTask = transmitter.StartAsync(cts.Token);
    transmitter.Transmit(bytes);

    startTask.Wait(TimeSpan.FromSeconds(30), cts.Token);

    Console.WriteLine("Stopping transmission and advertisement receiving...");
    advertisementReceiver.StopAsync(cts.Token).Wait();
    transmitter.StopAsync(cts.Token).Wait();
}
else
{
    Console.WriteLine("You are running on 1 - receiver");

    var receiver = new WindowsReceiver(receiverSettings);

    receiver.StartAsync(cts.Token).Wait();
    Console.WriteLine("Waiting for data from peer (60 seconds maximum)...");
    byte[]? data;

   
    Console.WriteLine("Starting publishing advertisements...");
    var advertisementPublisher = new WindowsBleAdvertisementTransmitter(new OptionsMock<AdvertisingSettings>(advSettings.CurrentValue));
    
    advertisementPublisher.StartAsync(cts.Token).Wait();
    
    Console.WriteLine("Trying to get messages...");
    while (!receiver.TryGetLast(out data))
    {
        Console.Write(".");
        Thread.Sleep(1000);
    }

    if (data == null || data.Length == 0)
        Console.WriteLine("No data available.");
    else
        Console.WriteLine($"Data available: {Encoding.UTF8.GetString(data)}");
    
    Console.WriteLine("Stop trying...");
}

cts.Cancel();
cts.Dispose();