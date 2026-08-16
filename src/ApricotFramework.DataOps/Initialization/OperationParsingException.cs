namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Thrown when an operations definition cannot be read, whether because the document is not
/// well-formed or because it does not satisfy the schema.
/// </summary>
public class OperationParsingException : Exception
{
    /// <summary>
    /// Creates an exception with no message.
    /// </summary>
    public OperationParsingException()
    {
    }

    /// <summary>
    /// Creates an exception with a message.
    /// </summary>
    /// <param name="message">The message.</param>
    public OperationParsingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates an exception with a message and a cause.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The cause.</param>
    public OperationParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
