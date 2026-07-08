namespace BLE.Messages;

/// <summary>
///     Represents a BLE message carrying application data.
/// </summary>
public class DataMessage : Message<DataMessage>
{
    /// <summary>
    ///     Creates a new <see cref="DataMessage" /> instance.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="data">Payload data.</param>
    protected DataMessage(byte[] id, byte[] data, byte bleVersion) : base(id, data, MessageType.Data, bleVersion)
    {
    }

    /// <summary>
    ///     Factory method for creating a <see cref="DataMessage" />.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="data">Payload data.</param>
    /// <param name="bleVersion">BLE version</param>
    /// <returns>A new <see cref="DataMessage" />.</returns>
    public static DataMessage Create(byte[] id, byte[] data, byte bleVersion)
    {
        return new DataMessage(id, data, bleVersion);
    }
}