namespace BLE.Messages;

/// <summary>
/// Represents a BLE receipt/acknowledgement message.
/// </summary>
public class ReceiptMessage : Message<ReceiptMessage>
{
    /// <summary>
    /// Creates a new <see cref="ReceiptMessage"/> instance.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="data">Receipt payload data.</param>
    protected ReceiptMessage(byte[] id, byte[] data) : base(id, data, MessageType.Receipt)
    {
    }

    /// <summary>
    /// Factory method for creating a <see cref="ReceiptMessage"/>.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="data">Receipt payload data.</param>
    /// <returns>A new <see cref="ReceiptMessage"/>.</returns>
    public static ReceiptMessage Create(byte[] id, byte[] data) => new ReceiptMessage(id, data);
}