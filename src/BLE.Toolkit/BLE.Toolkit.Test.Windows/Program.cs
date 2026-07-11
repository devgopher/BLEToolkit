// See https://aka.ms/new-console-template for more information

using System.Text;
using BLE.Toolkit;
using BLE.Toolkit.Settings;
using BLE.Toolkit.Windows.Receiver;
using BLE.Toolkit.Windows.Transmitter;

string serviceGuid = "0497947e-a031-491b-b1a0-163d605003d5";
string gattCharId = "16b7c725-ac93-4d29-b10e-039042971498";

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

    var transmitter = new CentralTransmitter(new OptionsMock<TransmitterSettings>(new TransmitterSettings
    {
        ProtocolVersion = ProtocolVersion.BLE5,
        TransmitQueueSize = 10,
        RetryPolicy = new RetryPolicySettings
        {
            RetryCount = 3,
            RetryDelay = TimeSpan.FromSeconds(1)
        },
        QueueFilledStrategy = QueueFilledStrategy.DequeueLast,
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
                    Characteristics = new ()
                    {
                        {"gattChar", gattCharId}
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
    }));
    
    transmitter.StartAsync(cts.Token).Wait();
    transmitter.Transmit(bytes);
    transmitter.StopAsync(cts.Token).Wait();
}
else
{
    Console.WriteLine("You are running on 1 - receiver");

    var receiver = new Receiver(new OptionsMock<ReceiverSettings>(new ReceiverSettings
    {
        ProtocolVersion = ProtocolVersion.BLE5,
        ReceiveQueueSize = 1,
        QueueFilledStrategy = QueueFilledStrategy.DequeueLast,
        ServiceSettings = new GattServiceSettings
        {
            Services =
            [
                new GattServiceSetting
                {
                    ServiceUuid = serviceGuid,
                    Characteristics = new()
                    {
                        { "gattChar", gattCharId }
                    }
                }
            ]
        },
    }));
    
    receiver.StartAsync(cts.Token).Wait();
    Console.WriteLine("Waiting for data from peer (60 seconds maximum)...");
    byte[]? data;
    
    while (!receiver.TryGetLast(out data))
    {
        Console.Write(".");
        Thread.Sleep(1000);
    }

    if (data == null || data.Length == 0)
    {
        Console.WriteLine("No data available.");
    }
    else
    {
        Console.WriteLine($"Data available: {Encoding.UTF8.GetString(data)}");
    }
}
    
cts.Cancel();