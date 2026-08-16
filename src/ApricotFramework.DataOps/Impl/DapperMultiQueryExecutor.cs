using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Runs an operation that returns several result sets.
/// </summary>
public class DapperMultiQueryExecutor : DapperOperationExecutorBase<IMultiQueryExecutor>, IMultiQueryExecutor
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    public DapperMultiQueryExecutor(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, operation)
    {
    }

    /// <inheritdoc />
    protected override IMultiQueryExecutor GetThis()
    {
        return this;
    }

    /// <inheritdoc />
    public Task<TResult> ExecuteAsync<TResult>(Func<IMultiResult, Task<TResult>> resultHandler, object? param = null)
    {
        ArgumentNullException.ThrowIfNull(resultHandler);

        return this.MaybeTransactionalAsync(async (connection, transaction) =>
        {
            var queryResult = await connection.QueryMultipleAsync(
                this.GetEffectiveSource(),
                param,
                transaction,
                this.GetEffectiveTimeout(),
                this.GetEffectiveCommandType()).ConfigureAwait(false);

            await using var reader = new DapperMultiResult(queryResult, this.Buffered);

            return await resultHandler(reader).ConfigureAwait(false);
        });
    }
}
