using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Interfaces.Receiver;
using BLE.Toolkit.Interfaces.Transmitter;
using BLE.Toolkit.Windows.Advertisement;
using BLE.Toolkit.Windows.Extensions;
using BLE.Toolkit.Windows.Receiver;
using BLE.Toolkit.Windows.Transmitter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLE.Toolkit.Windows.Tests.Extensions;

public class ServicesExtensionsTests
{
    [Fact]
    public void AddWindowsBleToolkitCentralNode_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddWindowsBleToolkitCentralNode(configuration);

        AssertRegisteredSingleton<IAdvertisementTransmitter, WindowsBleAdvertisementTransmitter>(services);
        AssertRegisteredSingleton<IAdvertisementReceiver, WindowsBleAdvertisementReceiver>(services);
        AssertRegisteredSingleton<IReceiver, WindowsReceiver>(services);
        AssertRegisteredSingleton<ITransmitter, CentralTransmitter>(services);
        Assert.Contains(services, d => d.ServiceType == typeof(DeviceCache) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddWindowsBleToolkitSlaveNode_RegistersServerNotifyTransmitter()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddWindowsBleToolkitSlaveNode(configuration);

        AssertRegisteredSingleton<ITransmitter, ServerNotifyTransmitter>(services);
        AssertRegisteredSingleton<IAdvertisementTransmitter, WindowsBleAdvertisementTransmitter>(services);
        AssertRegisteredSingleton<IAdvertisementReceiver, WindowsBleAdvertisementReceiver>(services);
        AssertRegisteredSingleton<IReceiver, WindowsReceiver>(services);
    }

    private static void AssertRegisteredSingleton<TService, TImplementation>(IServiceCollection services)
    {
        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(TService) && d.ImplementationType == typeof(TImplementation));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
