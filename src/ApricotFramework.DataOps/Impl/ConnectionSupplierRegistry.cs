namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// A connection registry whose data sources are registered in code rather than read from
/// configuration, for hosts that do not use the ASP.NET Core integration.
/// </summary>
public class ConnectionSupplierRegistry : IConnectionRegistry
{
    private readonly string defaultDataSource;
    private readonly Dictionary<string, Func<IConnectionReference>> suppliers = [];

    /// <summary>
    /// Creates a registry with no data sources.
    /// </summary>
    /// <param name="defaultDataSource">The data source used when a caller does not name one.</param>
    public ConnectionSupplierRegistry(string defaultDataSource)
    {
        this.defaultDataSource = defaultDataSource;
    }

    /// <summary>
    /// Registers a data source, replacing any already registered under that name.
    /// </summary>
    /// <param name="dataSource">The data source name, or null for the default.</param>
    /// <param name="supplier">Creates a closed connection each time it is called.</param>
    public void Register(string? dataSource, Func<IConnectionReference> supplier)
    {
        this.suppliers[dataSource ?? this.GetDefaultDataSource()] = supplier;
    }

    /// <inheritdoc />
    public virtual string GetDefaultDataSource()
    {
        return this.defaultDataSource;
    }

    /// <inheritdoc />
    public IConnectionReference? CreateOrNull(string? dataSource)
    {
        var supplier = this.suppliers.GetValueOrDefault(dataSource ?? this.GetDefaultDataSource());
        return supplier?.Invoke();
    }
}
