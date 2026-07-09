namespace BLE.Toolkit.Settings;

/// <summary>
///     Behavior when queue is filled
/// </summary>
public enum QueueFilledStrategy
{
    /// <summary>
    ///     Dequeues last message
    /// </summary>
    DequeueLast,

    /// <summary>
    ///     Throws an exception
    /// </summary>
    ThrowException
}