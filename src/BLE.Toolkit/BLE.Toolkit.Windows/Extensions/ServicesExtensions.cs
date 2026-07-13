using BLE.Toolkit.Advertisement;
using BLE.Toolkit.Cache;
using BLE.Toolkit.Interfaces.Receiver;
using BLE.Toolkit.Interfaces.Transmitter;
using BLE.Toolkit.Windows.Advertisement;
using BLE.Toolkit.Windows.Receiver;
using BLE.Toolkit.Windows.Transmitter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLE.Toolkit.Windows.Extensions;

/// <summary>
/// Extension methods that register the BLE‑toolkit services in an <see cref="IServiceCollection"/>
/// for use with the built‑in Microsoft dependency‑injection container.
/// Each method adds the core BLE components (transmitter, receiver, cache, etc.) and
/// optionally wires up a role‑specific <c>ITransmitter</c> implementation
/// (central node or slave node).
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Registers the common BLE‑toolkit services required by both central and slave nodes.
    /// </summary>
    /// <param name="services">The service collection to which the services are added.</param>
    /// <param name="configuration">
    /// Application configuration (e.g., <c>IConfiguration</c>) that can be used by the
    /// concrete implementations for reading settings such as device names, advertisement
    /// intervals, etc. The current implementation does not consume it directly, but it is
    /// passed to keep the signature consistent with other registration helpers.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent chaining.</returns>
    private static IServiceCollection AddBleToolkit(this IServiceCollection services,
        IConfiguration configuration)
        => services
            // Transmits BLE advertisements (platform‑specific implementation for Windows)
            .AddSingleton<IAdvertisementTransmitter, WindowsBleAdvertisementTransmitter>()
            // Receives BLE advertisements (platform‑specific implementation for Windows)
            .AddSingleton<IAdvertisementReceiver, WindowsBleAdvertisementReceiver>()
            // Higher‑level receiver that processes raw advertisements
            .AddSingleton<IReceiver, WindowsReceiver>()
            // In‑memory cache that stores discovered devices and their metadata
            .AddSingleton<DeviceCache>();

    /// <summary>
    /// Registers the BLE‑toolkit services for a **central node** (i.e., a device that initiates
    /// connections and sends notifications to slaves). In addition to the core services,
    /// it registers <see cref="CentralTransmitter"/> as the concrete <see cref="ITransmitter"/>
    /// implementation.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configuration">Configuration source (passed to <c>AddBleToolkit</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddWindowsBleToolkitCentralNode(this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddBleToolkit(configuration)                       // core BLE services
            .AddSingleton<ITransmitter, CentralTransmitter>(); // central‑node specific transmitter

    /// <summary>
    /// Registers the BLE‑toolkit services for a **slave node** (i.e., a device that responds
    /// to a central node and receives notifications). In addition to the core services,
    /// it registers <see cref="ServerNotifyTransmitter"/> as the concrete <see cref="ITransmitter"/>
    /// implementation.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configuration">Configuration source (passed to <c>AddBleToolkit</c>).</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddWindowsBleToolkitSlaveNode(this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddBleToolkit(configuration)                        // core BLE services
            .AddSingleton<ITransmitter, ServerNotifyTransmitter>(); // slave‑node specific transmitter
}