namespace ApricotFramework.DataOps;

/// <summary>
/// The transaction an operation opens around itself.
/// </summary>
/// <remarks>
/// An unspecified value inherits from the enclosing group and then the file-wide configuration;
/// <see cref="No"/> stops that inheritance and runs the operation without a transaction.
/// </remarks>
public enum AutoTransaction
{
    /// <summary>
    /// Run without a transaction, overriding anything inherited.
    /// </summary>
    No,

    /// <summary>
    /// Open a transaction at the repeatable read isolation level.
    /// </summary>
    RepeatableRead,

    /// <summary>
    /// Open a transaction at the read uncommitted isolation level.
    /// </summary>
    ReadUncommitted,

    /// <summary>
    /// Open a transaction at the read committed isolation level.
    /// </summary>
    ReadCommitted,

    /// <summary>
    /// Open a transaction at the serializable isolation level.
    /// </summary>
    Serializable
}
