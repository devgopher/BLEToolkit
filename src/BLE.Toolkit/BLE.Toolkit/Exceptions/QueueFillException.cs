using System.Runtime.Serialization;

namespace BLE.Toolkit.Exceptions;

public class QueueFillException : Exception
{
    public QueueFillException()
    {
    }

    public QueueFillException(string? message) : base(message)
    {
    }

    public QueueFillException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}