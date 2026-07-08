namespace BLE.Messages.Parsers;

public interface IDeserializer
{
    public MessageType GetType(byte[] input);
    public byte? BleVersion(byte[] input);
    public Message<T>? GetMessage<T>(byte[] input) where T : Message<T>;
}