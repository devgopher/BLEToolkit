namespace BLE.Messages.Parsers;

public interface ISerializer
{
    /// <summary>
    /// Serializes the provided message into a BLE frame.
    /// Format: [type(1)][id][data] — BLE4 id: 4 bytes, BLE5 id: 32 bytes.
    /// </summary>
    byte[] Serialize(MessageType type, byte bleVersion, byte[] id, byte[] data);

    /// <summary>
    /// Serializes a concrete <see cref="Message{T}"/> instance.
    /// </summary>
    byte[] Serialize<T>(Message<T> message, byte bleVersion) where T : Message<T>;
}