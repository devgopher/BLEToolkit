namespace BLE.Messages.Parsers;

public class Deserializer : IDeserializer
{
    public MessageType GetType(byte[] input)
    {
        return (MessageType)input[0];
    }

    public byte? BleVersion(byte[] input)
    {
        return input[1];
    }

    public Message<T>? GetMessage<T>(byte[] input) where T : Message<T>
    {
        var type = GetType(input);
        var version = BleVersion(input);

        return type switch
        {
            MessageType.Data => version switch
            {
                4 => DataMessage.Create(input[1..4], input[5..], 4) as Message<T>,
                5 => DataMessage.Create(input[1..33], input[34..], 5) as Message<T>,
                _ => DataMessage.Create(input[1..4], input[5..], 4) as Message<T>
            },
            MessageType.Receipt => version switch
            {
                4 => ReceiptMessage.Create(input[1..4], 4) as Message<T>,
                5 => ReceiptMessage.Create(input[1..33], 5) as Message<T>,
                _ => ReceiptMessage.Create(input[1..4], 4) as Message<T>
            },
            MessageType.ProtocolsApproval => version switch
            {
                4 => ProtocolsApprovalMessage.Create(input[1..4], 4) as Message<T>,
                5 => ProtocolsApprovalMessage.Create(input[1..33], 5) as Message<T>,
                _ => ProtocolsApprovalMessage.Create(input[1..4], 4) as Message<T>
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}