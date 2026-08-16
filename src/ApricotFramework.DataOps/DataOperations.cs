using System.Data;
using System.Data.Common;
using ApricotFramework.DataOps.Impl;

namespace ApricotFramework.DataOps;

/// <summary>
/// The entry point: connects to a data source and routes to the operations defined for it.
/// </summary>
public class DataOperations : IDataOperations
{
    private readonly IOperationRegistry operationRegistry;
    private readonly IConnectionRegistry connectionRegistry;

    /// <summary>
    /// Creates an entry point over an operation registry and a connection registry.
    /// </summary>
    /// <param name="operationRegistry">The operations available to route to.</param>
    /// <param name="connectionRegistry">The data sources available to connect to.</param>
    public DataOperations(IOperationRegistry operationRegistry, IConnectionRegistry connectionRegistry)
    {
        this.operationRegistry = operationRegistry;
        this.connectionRegistry = connectionRegistry;
    }

    /// <inheritdoc />
    public IOperationRouter Connect(string dataSource)
    {
        return new OperationRouter(this.ConnectDirect(dataSource), this.operationRegistry);
    }

    /// <inheritdoc />
    public IOperationRouter Connect()
    {
        return this.Connect(this.GetDataSource());
    }

    /// <inheritdoc />
    public IOperationRouter Connect(IDbConnection connection, SqlProvider provider)
    {
        return new OperationRouter(ConnectionReference.Borrowing(connection, provider), this.operationRegistry);
    }

    /// <inheritdoc />
    public IConnectionReference ConnectDirect()
    {
        return this.ConnectDirect(this.GetDataSource());
    }

    /// <inheritdoc />
    public IConnectionReference ConnectDirect(string dataSource)
    {
        return this.connectionRegistry.CreateOrNull(dataSource)
            ?? throw new IncorrectOperationException($"The data source '{dataSource}' is requested but not configured.");
    }

    /// <inheritdoc />
    public Task<ITransactionScope> BeginAsync(AutoTransaction isolation = AutoTransaction.ReadCommitted, CancellationToken cancellationToken = default)
    {
        return this.BeginAsync(this.GetDataSource(), isolation, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ITransactionScope> BeginAsync(string dataSource, AutoTransaction isolation = AutoTransaction.ReadCommitted, CancellationToken cancellationToken = default)
    {
        var reference = this.ConnectDirect(dataSource);

        try
        {
            await OpenAsync(reference.Connection, cancellationToken).ConfigureAwait(false);

            var transaction = reference.Connection.BeginTransaction(MapIsolation(isolation));

            return new OperationTransactionScope(reference, transaction, this.operationRegistry);
        }
        catch (Exception)
        {
            await reference.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Gets the data source to use when a caller does not name one.
    /// </summary>
    /// <returns>The data source name.</returns>
    protected virtual string GetDataSource()
    {
        return this.connectionRegistry.GetDefaultDataSource();
    }

    private static Task OpenAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is DbConnection dbConnection)
        {
            return dbConnection.OpenAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        connection.Open();
        return Task.CompletedTask;
    }

    private static IsolationLevel MapIsolation(AutoTransaction isolation)
    {
        return isolation switch
        {
            AutoTransaction.RepeatableRead => IsolationLevel.RepeatableRead,
            AutoTransaction.ReadUncommitted => IsolationLevel.ReadUncommitted,
            AutoTransaction.ReadCommitted => IsolationLevel.ReadCommitted,
            AutoTransaction.Serializable => IsolationLevel.Serializable,
            _ => throw new ArgumentOutOfRangeException(nameof(isolation), isolation, "A transaction scope needs a real isolation level.")
        };
    }
}
