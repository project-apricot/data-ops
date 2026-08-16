using System.Data;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Resolves operation keys against the registry and builds Dapper executors for them.
/// </summary>
public sealed class OperationRouter : IOperationRouter
{
    private readonly IConnectionReference connectionReference;
    private readonly IOperationRegistry operationRegistry;
    private readonly IDbTransaction? ambientTransaction;
    private readonly bool ownsReference;

    /// <summary>
    /// Creates a router that owns its connection reference.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operationRegistry">The registry to resolve keys against.</param>
    public OperationRouter(IConnectionReference connectionReference, IOperationRegistry operationRegistry)
        : this(connectionReference, operationRegistry, ambientTransaction: null, ownsReference: true)
    {
    }

    /// <summary>
    /// Creates a router, optionally enlisting every operation in a transaction someone else owns.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operationRegistry">The registry to resolve keys against.</param>
    /// <param name="ambientTransaction">The transaction to enlist in, or null for none.</param>
    /// <param name="ownsReference">Whether disposing the router disposes the reference.</param>
    public OperationRouter(
        IConnectionReference connectionReference,
        IOperationRegistry operationRegistry,
        IDbTransaction? ambientTransaction,
        bool ownsReference)
    {
        this.connectionReference = connectionReference;
        this.operationRegistry = operationRegistry;
        this.ambientTransaction = ambientTransaction;
        this.ownsReference = ownsReference;
    }

    /// <inheritdoc />
    public IQueryExecutor Query(OpKey key)
    {
        var executor = new DapperQueryExecutor(this.connectionReference, this.GetValidOrThrow(key, ResultType.Table));
        return this.ambientTransaction is null ? executor : executor.WithTransaction(this.ambientTransaction);
    }

    /// <inheritdoc />
    public IQueryFirstExecutor QueryFirst(OpKey key)
    {
        var executor = new DapperQueryFirstExecutor(this.connectionReference, this.GetValidOrThrow(key, ResultType.Table));
        return this.ambientTransaction is null ? executor : executor.WithTransaction(this.ambientTransaction);
    }

    /// <inheritdoc />
    public INonQueryExecutor NonQuery(OpKey key)
    {
        var executor = new DapperNonQueryExecutor(this.connectionReference, this.GetValidOrThrow(key, ResultType.RowCount));
        return this.ambientTransaction is null ? executor : executor.WithTransaction(this.ambientTransaction);
    }

    /// <inheritdoc />
    public IMultiQueryExecutor MultiQuery(OpKey key)
    {
        var executor = new DapperMultiQueryExecutor(this.connectionReference, this.GetValidOrThrow(key, ResultType.MultipleTables));
        return this.ambientTransaction is null ? executor : executor.WithTransaction(this.ambientTransaction);
    }

    /// <inheritdoc />
    public IScalarExecutor Scalar(OpKey key)
    {
        var executor = new DapperScalarExecutor(this.connectionReference, this.GetValidOrThrow(key, ResultType.Scalar));
        return this.ambientTransaction is null ? executor : executor.WithTransaction(this.ambientTransaction);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this.ownsReference)
        {
            this.connectionReference.Dispose();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (this.ownsReference)
        {
            await this.connectionReference.DisposeAsync().ConfigureAwait(false);
        }
    }

    private SqlOperation GetValidOrThrow(OpKey key, ResultType resultType)
    {
        if (!this.operationRegistry.IsDefined(key))
        {
            throw new IncorrectOperationException($"The operation {key} is not defined in the registry.");
        }

        var operation = this.operationRegistry.GetOrNull(key, this.connectionReference.Provider);

        if (operation is null)
        {
            throw new IncorrectOperationException($"The operation {key} is not supported for provider {this.connectionReference.Provider}.");
        }

        // An operation that declares nothing may be called through any executor.
        if (operation.ResultType != resultType && operation.ResultType != ResultType.Unknown)
        {
            throw new IncorrectOperationException($"The operation {key} is defined to result {operation.ResultType} but result {resultType} is required in this context.");
        }

        return operation;
    }
}
