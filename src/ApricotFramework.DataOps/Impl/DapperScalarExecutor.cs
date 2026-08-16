using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Runs an operation that returns a single value.
/// </summary>
public class DapperScalarExecutor : DapperOperationExecutorBase<IScalarExecutor>, IScalarExecutor
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    public DapperScalarExecutor(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, operation)
    {
    }

    /// <inheritdoc />
    protected override IScalarExecutor GetThis()
    {
        return this;
    }

    /// <inheritdoc />
    public Task<TResult?> ExecuteAsync<TResult>(object? param = null)
    {
        return this.MaybeTransactionalAsync((connection, transaction) => connection.ExecuteScalarAsync<TResult>(
            this.GetEffectiveSource(),
            param,
            transaction,
            this.GetEffectiveTimeout(),
            this.GetEffectiveCommandType()));
    }
}
