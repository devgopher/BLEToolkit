namespace BLE.Messages.Validation;

/// <summary>
/// Generic validator implementation (protocol-agnostic).
/// </summary>
/// <typeparam name="TMessage">The message type to validate.</typeparam>
public class GenericValidator<TMessage> : IValidator<TMessage>
    where TMessage : Message<TMessage>
{
    /// <summary>
    /// Validates the provided message instance with generic rules.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <returns><c>true</c> if the message is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(TMessage? message)
        => message != null && message?.Id.Length > 0 && message?.Data.Length > 0;
}