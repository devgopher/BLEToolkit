namespace BLE.Messages.Validation;

/// <summary>
///     Validator implementation intended for BLE5-specific validation rules.
/// </summary>
/// <typeparam name="TMessage">The message type to validate.</typeparam>
public class BLE5Validator<TMessage> : IValidator<TMessage>
    where TMessage : Message<TMessage>
{
    /// <summary>
    ///     Validates the provided message instance according to BLE5 rules.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <returns><c>true</c> if the message is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(TMessage? message)
    {
        return message is { Data: { Length: 221 }, Id.Length: 32 };
    }
}