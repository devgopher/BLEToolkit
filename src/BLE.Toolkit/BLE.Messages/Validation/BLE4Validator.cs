namespace BLE.Messages.Validation;

/// <summary>
/// Validator implementation intended for BLE4-specific validation rules.
/// </summary>
/// <typeparam name="TMessage">The message type to validate.</typeparam>
public class BLE4Validator<TMessage> : IValidator<TMessage>
    where TMessage : Message<TMessage>
{
    /// <summary>
    /// Validates the provided message instance according to BLE4 rules.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <returns><c>true</c> if the message is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid(TMessage? message) => message is { Data: { Length: 19 }, Id.Length: 4 };
}