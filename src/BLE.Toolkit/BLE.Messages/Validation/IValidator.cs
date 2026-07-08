namespace BLE.Messages.Validation;

/// <summary>
/// Defines a validator for BLE messages of type <typeparamref name="TMessage"/>.
/// </summary>
/// <typeparam name="TMessage">The message type to validate.</typeparam>
public interface IValidator<in TMessage>
    where TMessage : Message<TMessage>
{
    /// <summary>
    /// Validates the provided message instance.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <returns><c>true</c> if the message is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(TMessage? message);
}