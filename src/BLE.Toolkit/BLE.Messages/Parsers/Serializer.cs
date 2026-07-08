namespace BLE.Messages.Parsers;

public class Serializer : ISerializer
{
    /// <summary>
    /// Serializes the provided message into a BLE frame.
    /// Format: [type(1)][bleVersion(1)][payload(variable)]
    /// </summary>
    public byte[] Serialize(MessageType type, byte bleVersion, byte[] id, byte[] data)
    {
        switch (bleVersion)
        {
            // Frame layout used by your deserializer:
            // - For BLE4: id = input[1..4] => 4 bytes, data begins at index 4
            // - For BLE5: id = input[1..33] => 32 bytes, data begins at index 33
            // For BLE4 your deserializer expects:
            // payload: id(4 bytes) + data(rest)
            case 4 when id.Length != 4:
                throw new ArgumentException("For BLE4, Id must be 4 bytes.");
            case 4:
            {
                var result = new byte[2 + id.Length + data.Length];
                result[0] = (byte)type;
                result[1] = bleVersion;

                Buffer.BlockCopy(id, 0, result, 2, id.Length);
                Buffer.BlockCopy(data, 0, result, 2 + id.Length, data.Length);
                return result;
            }
            // For BLE5 your deserializer expects:
            // payload: id(32 bytes) + data(rest)
            case 5 when id.Length != 32:
                throw new ArgumentException("For BLE5, Id must be 32 bytes.");
            case 5:
            {
                var result = new byte[2 + id.Length + data.Length];
                result[0] = (byte)type;
                result[1] = bleVersion;

                Buffer.BlockCopy(id, 0, result, 2, id.Length);
                Buffer.BlockCopy(data, 0, result, 2 + id.Length, data.Length);
                return result;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(bleVersion), "Unsupported BLE version. Use 4 or 5.");
        }
    }

    /// <summary>
    /// Serializes a concrete <see cref="Message{T}"/> instance.
    /// </summary>
    public byte[] Serialize<T>(Message<T> message, byte bleVersion) where T : Message<T>
        => Serialize(message.Type, bleVersion, message.Id, message.Data);
}