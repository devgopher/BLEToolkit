namespace BLE.Toolkit.Sample.Load.Models;

public enum NodeRole
{
    None,
    CentralTransmitter,
    ServerNotifyTransmitter,
    Receiver
}

public sealed record SetRoleRequest(string Role);

public sealed record TransmitRequest(string Message, int Count);

public sealed record ReceivedMessageDto(int Index, string Text, DateTimeOffset ReceivedAt);

public sealed record ReceiverMessagesResponse(int TotalCount, IReadOnlyList<ReceivedMessageDto> Messages);

public sealed record ReceiverCountResponse(int Count);

public sealed record SetThrottlingRequest(bool Enabled, string RatePeriod, ushort Limit);

public sealed record ThrottlingSettingsDto(bool Enabled, string RatePeriod, ushort Limit);

public sealed record CachedDeviceDto(string BluetoothAddress, string? LocalName);

public sealed record TransmitterStatusDto(
    string Mode,
    int TargetCount,
    int EnqueuedCount,
    int DiscoveredDevices,
    string? LastMessage,
    ThrottlingSettingsDto Throttling,
    IReadOnlyList<CachedDeviceDto> Devices);

public sealed record ReceiverStatusDto(int ReceivedCount);

public sealed record NodeStatusResponse(
    string Role,
    bool IsRunning,
    TransmitterStatusDto? Transmitter,
    ReceiverStatusDto? Receiver);
