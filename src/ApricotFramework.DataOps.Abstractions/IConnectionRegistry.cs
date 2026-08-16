namespace ApricotFramework.DataOps;

/// <summary>
/// Resolves a named data source into a connection.
/// </summary>
public interface IConnectionRegistry
{
    /// <summary>
    /// Gets the data source used when a caller does not name one.
    /// </summary>
    /// <returns>The default data source name.</returns>
    string GetDefaultDataSource();

    /// <summary>
    /// Creates a connection for a data source without opening it.
    /// </summary>
    /// <param name="dataSource">The data source name, or null for the default.</param>
    /// <returns>An owning reference, or null when no such data source is configured.</returns>
    IConnectionReference? CreateOrNull(string? dataSource);
}
