using System.Data;

namespace ApricotFramework.DataOps.AspNetCore.Impl;

/// <summary>
/// A connection factory built from a lambda, so a host can bind a provider to its chosen driver
/// without writing a class.
/// </summary>
public sealed class DelegateDbConnectionFactory : IDbConnectionFactory
{
    private readonly SqlProvider provider;
    private readonly Func<string, IDbConnection> factory;

    /// <summary>
    /// Creates a factory for one provider.
    /// </summary>
    /// <param name="provider">The provider of this factory builds connections for.</param>
    /// <param name="factory">Builds a closed connection from a connection string.</param>
    public DelegateDbConnectionFactory(SqlProvider provider, Func<string, IDbConnection> factory)
    {
        this.provider = provider;
        this.factory = factory;
    }

    /// <inheritdoc />
    public SqlProvider GetProvider()
    {
        return this.provider;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection(string connectionString)
    {
        return this.factory(connectionString);
    }
}
