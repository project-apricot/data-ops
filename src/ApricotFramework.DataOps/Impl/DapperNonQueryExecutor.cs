using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Runs an operation that reports how many rows it affected.
/// </summary>
public class DapperNonQueryExecutor : DapperOperationExecutorBase<INonQueryExecutor>, INonQueryExecutor
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    public DapperNonQueryExecutor(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, operation)
    {
    }

    /// <inheritdoc />
    protected override INonQueryExecutor GetThis()
    {
        return this;
    }

    /// <inheritdoc />
    public Task<int> ExecuteAsync(object? param = null)
    {
        return this.MaybeTransactionalAsync((connection, transaction) => connection.ExecuteAsync(
            this.GetEffectiveSource(),
            param,
            transaction,
            this.GetEffectiveTimeout(),
            this.GetEffectiveCommandType()));
    }
}
