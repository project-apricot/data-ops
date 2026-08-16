namespace ApricotFramework.DataOps;

/// <summary>
/// An open connection and transaction that several operations run in.
/// </summary>
/// <remarks>
/// Every executor the scope's router hands out is enlisted in the transaction and will not commit
/// it, whatever the operation's own definition declares. Disposing without committing rolls back.
/// </remarks>
public interface ITransactionScope : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the router whose operations run in this transaction.
    /// </summary>
    IOperationRouter Router { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <returns>A task that completes once committed.</returns>
    Task CommitAsync();

    /// <summary>
    /// Rolls the transaction back.
    /// </summary>
    /// <returns>A task that completes once rolled back.</returns>
    Task RollbackAsync();
}
