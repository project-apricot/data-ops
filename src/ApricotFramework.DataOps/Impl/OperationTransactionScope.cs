using System.Data;
using System.Data.Common;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// An open connection and transaction that the operations of one router share.
/// </summary>
public sealed class OperationTransactionScope : ITransactionScope
{
    private readonly IConnectionReference connectionReference;
    private readonly IDbTransaction transaction;
    private bool completed;

    /// <summary>
    /// Takes ownership of an open connection and the transaction started on it.
    /// </summary>
    /// <param name="connectionReference">The open connection.</param>
    /// <param name="transaction">The transaction started on it.</param>
    /// <param name="operationRegistry">The registry to resolve keys against.</param>
    public OperationTransactionScope(
        IConnectionReference connectionReference,
        IDbTransaction transaction,
        IOperationRegistry operationRegistry)
    {
        this.connectionReference = connectionReference;
        this.transaction = transaction;
        this.Router = new OperationRouter(connectionReference, operationRegistry, transaction, ownsReference: false);
    }

    /// <inheritdoc />
    public IOperationRouter Router { get; }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        if (this.transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.CommitAsync().ConfigureAwait(false);
        }
        else
        {
            this.transaction.Commit();
        }

        this.completed = true;
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        if (this.transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.RollbackAsync().ConfigureAwait(false);
        }
        else
        {
            this.transaction.Rollback();
        }

        this.completed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.RollbackIfIncomplete();
        this.transaction.Dispose();
        this.Router.Dispose();
        this.connectionReference.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        this.RollbackIfIncomplete();

        if (this.transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            this.transaction.Dispose();
        }

        await this.Router.DisposeAsync().ConfigureAwait(false);
        await this.connectionReference.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Leaving the scope without committing abandons the work, so roll it back explicitly rather
    /// than relying on the provider to do it when the connection closes.
    /// </summary>
    private void RollbackIfIncomplete()
    {
        if (this.completed)
        {
            return;
        }

        this.completed = true;
        this.transaction.Rollback();
    }
}
