using System.Data;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Holds the settings shared by every executor and runs the operation in the right transaction.
/// </summary>
/// <typeparam name="TExecutor">The concrete executor being configured.</typeparam>
public abstract class DapperExecutorBase<TExecutor> : IOperationExecutor<TExecutor>
{
    private readonly Dictionary<string, object?> bindings = [];

    /// <summary>
    /// Creates an executor bound to a connection.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="autoTransaction">The transaction the operation's definition declared.</param>
    /// <param name="timeout">The timeout the operation's definition declared.</param>
    protected DapperExecutorBase(IConnectionReference connectionReference, AutoTransaction? autoTransaction, TimeSpan? timeout)
    {
        this.ConnectionReference = connectionReference;
        this.AutoTransaction = Normalize(autoTransaction);
        this.Timeout = timeout;
        this.AutoCommit = this.AutoTransaction.HasValue;
        this.Buffered = true;
    }

    /// <inheritdoc />
    public IConnectionReference ConnectionReference { get; }

    /// <inheritdoc />
    public IDbTransaction? Transaction { get; private set; }

    /// <inheritdoc />
    public bool Buffered { get; private set; }

    /// <inheritdoc />
    public bool AutoCommit { get; private set; }

    /// <inheritdoc />
    public AutoTransaction? AutoTransaction { get; private set; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Bindings => this.bindings;

    /// <inheritdoc />
    public TExecutor WithTransaction(IDbTransaction? transaction, bool autocommit = false)
    {
        this.Transaction = transaction;
        this.AutoCommit = autocommit;
        this.AutoTransaction = null;
        return this.GetThis();
    }

    /// <inheritdoc />
    public TExecutor WithTransaction(AutoTransaction? transaction)
    {
        this.Transaction = null;
        this.AutoTransaction = Normalize(transaction);
        this.AutoCommit = this.AutoTransaction.HasValue;
        return this.GetThis();
    }

    /// <inheritdoc />
    public TExecutor WithTimeout(TimeSpan? timeout)
    {
        this.Timeout = timeout;
        return this.GetThis();
    }

    /// <inheritdoc />
    public TExecutor WithBuffering(bool buffered = true)
    {
        this.Buffered = buffered;
        return this.GetThis();
    }

    /// <inheritdoc />
    public TExecutor WithBinding(string binding, object? value)
    {
        this.bindings[binding] = value;
        return this.GetThis();
    }

    /// <summary>
    /// Gets this executor, typed for chaining.
    /// </summary>
    /// <returns>This executor.</returns>
    protected abstract TExecutor GetThis();

    /// <summary>
    /// Runs the operation, opening and committing a transaction around it when one is declared.
    /// </summary>
    /// <typeparam name="TResult">The result of the operation.</typeparam>
    /// <param name="function">Runs the command against the connection and transaction.</param>
    /// <returns>The result.</returns>
    protected async Task<TResult> MaybeTransactionalAsync<TResult>(Func<IDbConnection, IDbTransaction?, Task<TResult>> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        var connection = this.ConnectionReference.Connection;

        // A connection the caller already opened stays open and stays theirs to close.
        var wasClosed = connection.State == ConnectionState.Closed;

        var transaction = this.Transaction;
        IDbTransaction? ownTransaction = null;

        if (transaction is null && this.AutoTransaction.HasValue)
        {
            if (wasClosed)
            {
                connection.Open();
            }

            ownTransaction = connection.BeginTransaction(MapIsolation(this.AutoTransaction.Value));
            transaction = ownTransaction;
        }

        try
        {
            var result = await function(connection, transaction).ConfigureAwait(false);

            if (this.AutoCommit)
            {
                transaction?.Commit();
            }

            return result;
        }
        catch (Exception)
        {
            if (this.AutoCommit)
            {
                transaction?.Rollback();
            }

            throw;
        }
        finally
        {
            ownTransaction?.Dispose();

            if (wasClosed)
            {
                connection.Close();
            }
        }
    }

    /// <summary>
    /// Turns an explicit "no transaction" into an absent one, so the two are handled alike.
    /// </summary>
    private static AutoTransaction? Normalize(AutoTransaction? autoTransaction)
    {
        return autoTransaction == DataOps.AutoTransaction.No ? null : autoTransaction;
    }

    private static IsolationLevel MapIsolation(AutoTransaction transaction)
    {
        return transaction switch
        {
            DataOps.AutoTransaction.RepeatableRead => IsolationLevel.RepeatableRead,
            DataOps.AutoTransaction.ReadUncommitted => IsolationLevel.ReadUncommitted,
            DataOps.AutoTransaction.ReadCommitted => IsolationLevel.ReadCommitted,
            DataOps.AutoTransaction.Serializable => IsolationLevel.Serializable,
            _ => throw new IncorrectOperationException($"The isolation level {transaction} is not supported.")
        };
    }
}
