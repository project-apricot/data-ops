using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Runs an operation that returns a result set.
/// </summary>
public class DapperQueryExecutor : DapperOperationExecutorBase<IQueryExecutor>, IQueryExecutor
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    public DapperQueryExecutor(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, operation)
    {
    }

    /// <inheritdoc />
    protected override IQueryExecutor GetThis()
    {
        return this;
    }

    /// <inheritdoc />
    public Task<IEnumerable<TResult>> ExecuteAsync<TResult>(object? param = null)
    {
        return this.MaybeTransactionalAsync((connection, transaction) => connection.QueryAsync<TResult>(
            this.GetEffectiveSource(),
            param,
            transaction,
            this.GetEffectiveTimeout(),
            this.GetEffectiveCommandType()));
    }

    /// <inheritdoc />
    public Task<IEnumerable<TResult>> ExecuteAsync<TFirst, TSecond, TResult>(Func<TFirst, TSecond, TResult> map, string splitOn = "id", object? param = null)
    {
        return this.MaybeTransactionalAsync((connection, transaction) => connection.QueryAsync(
            this.GetEffectiveSource(),
            map,
            param,
            transaction,
            this.Buffered,
            splitOn,
            this.GetEffectiveTimeout(),
            this.GetEffectiveCommandType()));
    }
}
