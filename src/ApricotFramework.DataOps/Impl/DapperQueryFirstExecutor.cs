using System.Data;
using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Runs an operation that returns a single row.
/// </summary>
public class DapperQueryFirstExecutor : DapperOperationExecutorBase<IQueryFirstExecutor>, IQueryFirstExecutor
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    public DapperQueryFirstExecutor(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, operation)
    {
    }

    /// <inheritdoc />
    protected override IQueryFirstExecutor GetThis()
    {
        return this;
    }

    /// <inheritdoc />
    public Task<TResult?> ExecuteAsync<TResult>(object? param = null, QueryFirstMode mode = QueryFirstMode.FirstOrDefault)
    {
        var sql = this.GetEffectiveSource();
        var timeout = this.GetEffectiveTimeout();
        var commandType = this.GetEffectiveCommandType();

        return this.MaybeTransactionalAsync<TResult?>(async (connection, transaction) => mode switch
        {
            QueryFirstMode.First => await connection.QueryFirstAsync<TResult>(sql, param, transaction, timeout, commandType).ConfigureAwait(false),
            QueryFirstMode.SingleRow => await connection.QuerySingleAsync<TResult>(sql, param, transaction, timeout, commandType).ConfigureAwait(false),
            QueryFirstMode.SingleRowOrDefault => await connection.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, timeout, commandType).ConfigureAwait(false),
            _ => await connection.QueryFirstOrDefaultAsync<TResult>(sql, param, transaction, timeout, commandType).ConfigureAwait(false)
        });
    }
}
