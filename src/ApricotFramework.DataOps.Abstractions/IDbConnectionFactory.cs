using System.Data;

namespace ApricotFramework.DataOps;

/// <summary>
/// Builds connections for one provider, letting the host choose its own ADO.NET driver.
/// </summary>
/// <remarks>
/// Registering a factory is what binds a provider to a concrete driver. Without one the connection
/// registry falls back to <c>DbProviderFactories</c> and the data source's invariant name.
/// </remarks>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Gets the provider this factory builds connections for.
    /// </summary>
    /// <returns>The provider.</returns>
    SqlProvider GetProvider();

    /// <summary>
    /// Creates a closed connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The connection.</returns>
    IDbConnection CreateConnection(string connectionString);
}
