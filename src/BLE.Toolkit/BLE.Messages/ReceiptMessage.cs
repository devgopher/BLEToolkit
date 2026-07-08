namespace BLE.Messages;

/// <summary>
///     Represents a BLE receipt/acknowledgement message.
/// </summary>
public class ReceiptMessage : Message<ReceiptMessage>
{
    /// <summary>
    ///     Creates a new <see cref="ReceiptMessage" /> instance.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="bleVersion">BLE version</param>
    private ReceiptMessage(byte[] id, byte bleVersion) : base(id, [], MessageType.Receipt, bleVersion)
    {
    }

    /// <summary>
    ///     Factory method for creating a <see cref="ReceiptMessage" />.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="bleVersion">BLE version</param>
    /// <returns>A new <see cref="ReceiptMessage" />.</returns>
    public static ReceiptMessage Create(byte[] id, byte bleVersion)
    {
        return new ReceiptMessage(id, bleVersion);
    }
}