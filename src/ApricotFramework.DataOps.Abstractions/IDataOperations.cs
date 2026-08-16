using System.Data;

namespace ApricotFramework.DataOps;

/// <summary>
/// The entry point: connects to a data source and routes to the operations defined for it.
/// </summary>
public interface IDataOperations
{
    /// <summary>
    /// Connects to a named data source.
    /// </summary>
    /// <param name="dataSource">The data source name.</param>
    /// <returns>A router owning the connection it created.</returns>
    IOperationRouter Connect(string dataSource);

    /// <summary>
    /// Connects to the default data source.
    /// </summary>
    /// <returns>A router owning the connection it created.</returns>
    IOperationRouter Connect();

    /// <summary>
    /// Routes operations onto a connection the caller owns.
    /// </summary>
    /// <param name="connection">The connection, open or closed.</param>
    /// <param name="provider">The provider whose operations should be resolved.</param>
    /// <returns>A router that will not close or dispose of the connection.</returns>
    IOperationRouter Connect(IDbConnection connection, SqlProvider provider);

    /// <summary>
    /// Creates a connection for the default data source without opening it.
    /// </summary>
    /// <returns>A reference the caller owns and must dispose.</returns>
    IConnectionReference ConnectDirect();

    /// <summary>
    /// Creates a connection for a named data source without opening it.
    /// </summary>
    /// <param name="dataSource">The data source name.</param>
    /// <returns>A reference the caller owns and must dispose.</returns>
    IConnectionReference ConnectDirect(string dataSource);

    /// <summary>
    /// Opens the default data source and starts a transaction several operations can share.
    /// </summary>
    /// <param name="isolation">The isolation level to open at.</param>
    /// <param name="cancellationToken">Cancels opening the connection.</param>
    /// <returns>The scope, which rolls back if disposed without a commit.</returns>
    Task<ITransactionScope> BeginAsync(AutoTransaction isolation = AutoTransaction.ReadCommitted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a named data source and starts a transaction several operations can share.
    /// </summary>
    /// <param name="dataSource">The data source name.</param>
    /// <param name="isolation">The isolation level to open at.</param>
    /// <param name="cancellationToken">Cancels opening the connection.</param>
    /// <returns>The scope, which rolls back if disposed without a commit.</returns>
    Task<ITransactionScope> BeginAsync(string dataSource, AutoTransaction isolation = AutoTransaction.ReadCommitted, CancellationToken cancellationToken = default);
}
