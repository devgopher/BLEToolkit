namespace BLE.Messages.Parsers;

public class Deserializer : IDeserializer
{
    private const int Ble4IdLength = 4;
    private const int Ble5IdLength = 32;

    public MessageType GetType(byte[] input)
    {
        return (MessageType)input[0];
    }

    public byte? BleVersion(byte[] input)
    {
        return InferBleVersion(input, GetType(input));
    }

    public Message<T>? GetMessage<T>(byte[] input) where T : Message<T>
    {
        var type = GetType(input);
        var version = InferBleVersion(input, type);

        return type switch
        {
            MessageType.Data => version switch
            {
                4 => DataMessage.Create(input[1..5], input[5..], 4) as Message<T>,
                5 => DataMessage.Create(input[1..33], input[33..], 5) as Message<T>,
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported BLE version. Use 4 or 5.")
            },
            MessageType.Receipt => version switch
            {
                4 => ReceiptMessage.Create(input[1..5], 4) as Message<T>,
                5 => ReceiptMessage.Create(input[1..33], 5) as Message<T>,
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported BLE version. Use 4 or 5.")
            },
            MessageType.ProtocolsApproval => version switch
            {
                4 => ProtocolsApprovalMessage.Create(input[1..5], 4) as Message<T>,
                5 => ProtocolsApprovalMessage.Create(input[1..33], 5) as Message<T>,
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unsupported BLE version. Use 4 or 5.")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported message type.")
        };
    }

    private static byte InferBleVersion(byte[] input, MessageType type)
    {
        return type switch
        {
            MessageType.Data => input.Length switch
            {
                1 + Ble4IdLength + 17 => 4,
                1 + Ble5IdLength + 221 => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(input), "Cannot infer BLE version from data frame length.")
            },
            MessageType.Receipt or MessageType.ProtocolsApproval => input.Length switch
            {
                1 + Ble4IdLength => 4,
                1 + Ble5IdLength => 5,
                _ => throw new ArgumentOutOfRangeException(nameof(input), "Cannot infer BLE version from frame length.")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported message type.")
        };
    }
}
