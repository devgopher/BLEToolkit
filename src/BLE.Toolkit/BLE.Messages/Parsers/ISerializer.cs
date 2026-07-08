namespace BLE.Messages.Parsers;

public interface ISerializer
{
    /// <summary>
    /// Serializes the provided message into a BLE frame.
    /// Format: [type(1)][bleVersion(1)][payload(variable)]
    /// </summary>
    byte[] Serialize(MessageType type, byte bleVersion, byte[] id, byte[] data);

    /// <summary>
    /// Serializes a concrete <see cref="Message{T}"/> instance.
    /// </summary>
    byte[] Serialize<T>(Message<T> message, byte bleVersion) where T : Message<T>;
}