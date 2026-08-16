using System.Data;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// A connection reference that either owns its connection or borrows the caller's.
/// </summary>
public sealed class ConnectionReference : IConnectionReference
{
    private readonly bool owned;

    private ConnectionReference(IDbConnection connection, SqlProvider provider, bool owned)
    {
        this.Connection = connection;
        this.Provider = provider;
        this.owned = owned;
    }

    /// <inheritdoc />
    public IDbConnection Connection { get; }

    /// <inheritdoc />
    public SqlProvider Provider { get; }

    /// <summary>
    /// Wraps a connection this reference is responsible for disposing.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="provider">The provider it speaks to.</param>
    /// <returns>The reference.</returns>
    public static ConnectionReference Owning(IDbConnection connection, SqlProvider provider)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return new ConnectionReference(connection, provider, owned: true);
    }

    /// <summary>
    /// Wraps a connection someone else is responsible for disposing.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="provider">The provider it speaks to.</param>
    /// <returns>The reference.</returns>
    public static ConnectionReference Borrowing(IDbConnection connection, SqlProvider provider)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return new ConnectionReference(connection, provider, owned: false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this.owned)
        {
            this.Connection.Dispose();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!this.owned)
        {
            return;
        }

        if (this.Connection is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            return;
        }

        this.Connection.Dispose();
    }
}
