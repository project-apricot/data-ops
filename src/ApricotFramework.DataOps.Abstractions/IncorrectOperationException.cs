namespace ApricotFramework.DataOps;

/// <summary>
/// Thrown when an operation is addressed that is not defined, is not available for the connected
/// provider, or is called through an executor its declared result does not allow.
/// </summary>
public class IncorrectOperationException : Exception
{
    /// <summary>
    /// Creates an exception with no message.
    /// </summary>
    public IncorrectOperationException()
    {
    }

    /// <summary>
    /// Creates an exception with a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public IncorrectOperationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates an exception with a message and a cause.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The cause.</param>
    public IncorrectOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
