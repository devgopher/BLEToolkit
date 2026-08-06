using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Advertisement.Models;

namespace BLE.Toolkit.Tests.Advertisement;

public class BasicAdvertisementReceiverTests
{
    [Fact]
    public void OnAdvertisementReceived_RaisesAdvertisementReceivedEvent()
    {
        var receiver = new TestAdvertisementReceiver();
        BleAdvertisement? received = null;
        var advertisement = BleAdvertisementFactory.Create(
            bluetoothAddress: 0x112233445566,
            rssi: -70,
            kind: BleAdvertisementKind.ScanResponse,
            localName: "beacon");

        receiver.AdvertisementReceived += ad => received = ad;
        receiver.Raise(advertisement);

        Assert.Same(advertisement, received);
    }

    [Fact]
    public void OnAdvertisementReceived_WithoutSubscribers_DoesNotThrow()
    {
        var receiver = new TestAdvertisementReceiver();
        var advertisement = BleAdvertisementFactory.Create(bluetoothAddress: 1);

        var exception = Record.Exception(() => receiver.Raise(advertisement));

        Assert.Null(exception);
    }

    [Fact]
    public void OnAdvertisementReceived_InvokesAllSubscribers()
    {
        var receiver = new TestAdvertisementReceiver();
        var calls = 0;
        var advertisement = BleAdvertisementFactory.Create(bluetoothAddress: 2);

        receiver.AdvertisementReceived += _ => calls++;
        receiver.AdvertisementReceived += _ => calls++;
        receiver.Raise(advertisement);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task StartAndStop_AreInvoked()
    {
        var receiver = new TestAdvertisementReceiver();

        await receiver.StartAsync(CancellationToken.None);
        await receiver.StopAsync(CancellationToken.None);

        Assert.Equal(1, receiver.StartCount);
        Assert.Equal(1, receiver.StopCount);
    }

    private sealed class TestAdvertisementReceiver : BasicAdvertisementReceiver
    {
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public void Raise(BleAdvertisement advertisement) => OnAdvertisementReceived(advertisement);

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            return Task.CompletedTask;
        }
    }
}
