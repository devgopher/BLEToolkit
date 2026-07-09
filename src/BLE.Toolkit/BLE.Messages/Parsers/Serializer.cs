namespace BLE.Messages.Parsers;

public class Serializer : ISerializer
{
    /// <summary>
    ///     Serializes the provided message into a BLE frame.
    ///     Format: [type(1)][id][data]
    ///     BLE4 id: 4 bytes, BLE5 id: 32 bytes.
    /// </summary>
    public byte[] Serialize(MessageType type, byte bleVersion, byte[] id, byte[] data)
    {
        switch (bleVersion)
        {
            case 4 when id.Length != 4:
                throw new ArgumentException("For BLE4, Id must be 4 bytes.");
            case 4:
            {
                var result = new byte[1 + id.Length + data.Length];
                result[0] = (byte)type;

                Buffer.BlockCopy(id, 0, result, 1, id.Length);
                Buffer.BlockCopy(data, 0, result, 1 + id.Length, data.Length);
                return result;
            }
            case 5 when id.Length != 32:
                throw new ArgumentException("For BLE5, Id must be 32 bytes.");
            case 5:
            {
                var result = new byte[1 + id.Length + data.Length];
                result[0] = (byte)type;

                Buffer.BlockCopy(id, 0, result, 1, id.Length);
                Buffer.BlockCopy(data, 0, result, 1 + id.Length, data.Length);
                return result;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(bleVersion), "Unsupported BLE version. Use 4 or 5.");
        }
    }

    /// <summary>
    ///     Serializes a concrete <see cref="Message{T}" /> instance.
    /// </summary>
    public byte[] Serialize<T>(Message<T> message, byte bleVersion) where T : Message<T>
    {
        return Serialize(message.Type, bleVersion, message.Id, message.Data);
    }
}