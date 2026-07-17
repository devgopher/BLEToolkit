namespace BLE.Toolkit.Sample.Load.Models;

public enum NodeRole
{
    None,
    Transmitter,
    Receiver
}

public sealed record SetRoleRequest(string Role);

public sealed record TransmitRequest(string Message, int Count);

public sealed record ReceivedMessageDto(int Index, string Text, DateTimeOffset ReceivedAt);

public sealed record ReceiverMessagesResponse(int TotalCount, IReadOnlyList<ReceivedMessageDto> Messages);

public sealed record ReceiverCountResponse(int Count);

public sealed record TransmitterStatusDto(int TargetCount, int EnqueuedCount, int DiscoveredDevices, string? LastMessage);

public sealed record ReceiverStatusDto(int ReceivedCount);

public sealed record NodeStatusResponse(
    string Role,
    bool IsRunning,
    TransmitterStatusDto? Transmitter,
    ReceiverStatusDto? Receiver);
