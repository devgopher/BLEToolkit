namespace BLE.Messages;

/// <summary>
///     Represents a BLE message indicating that protocols have been approved.
/// </summary>
public class ProtocolsApprovalMessage : Message<ProtocolsApprovalMessage>
{
    /// <summary>
    ///     Creates a new <see cref="ProtocolsApprovalMessage" /> instance.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="bleVersion">BLE version</param>
    private ProtocolsApprovalMessage(byte[] id, byte bleVersion) : base(id, [],
        MessageType.ProtocolsApproval, bleVersion)
    {
    }

    /// <summary>
    ///     Factory method for creating a <see cref="ProtocolsApprovalMessage" />.
    /// </summary>
    /// <param name="id">Message identifier.</param>
    /// <param name="data">Approval payload data.</param>
    /// <param name="bleVersion">BLE version</param>
    /// <returns>A new <see cref="ProtocolsApprovalMessage" />.</returns>
    public static ProtocolsApprovalMessage Create(byte[] id, byte bleVersion)
    {
        return new ProtocolsApprovalMessage(id, bleVersion);
    }
}