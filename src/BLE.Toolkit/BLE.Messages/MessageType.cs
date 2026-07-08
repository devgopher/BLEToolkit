namespace BLE.Messages;

/// <summary>
/// Supported message types for BLE communication.
/// </summary>
public enum MessageType : byte
{
    /// <summary>Regular payload/message data.</summary>
    Data,

    /// <summary>Acknowledgement/response message.</summary>
    Receipt,

    /// <summary>Message indicating protocol approval/handshake approval.</summary>
    ProtocolsApproval
}