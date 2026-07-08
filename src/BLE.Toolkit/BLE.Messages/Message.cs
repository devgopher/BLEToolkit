namespace BLE.Messages;

/// <summary>
/// Base class for BLE messages.
/// </summary>
/// <typeparam name="T">The concrete message type (self-referential generic).</typeparam>
public abstract class Message<T>(byte[] id, byte[] data, MessageType type)
    where T : Message<T>
{
    /// <summary>
    /// Identifier of the message (typically used to correlate requests and responses).
    /// </summary>
    public byte[] Id { get; init; } = id;

    /// <summary>
    /// Message payload.
    /// </summary>
    public byte[] Data { get; init; } = data;

    /// <summary>
    /// The message type.
    /// </summary>
    public MessageType Type { get; init; } = type;
}