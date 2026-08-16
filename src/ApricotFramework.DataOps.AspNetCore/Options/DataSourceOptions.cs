namespace ApricotFramework.DataOps.AspNetCore.Options;

/// <summary>
/// One data source, as declared in the configuration.
/// </summary>
public class DataSourceOptions
{
    /// <summary>
    /// Gets or sets the provider whose operations this data source resolves.
    /// </summary>
    public SqlProvider? Provider { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the ADO.NET invariant name used to build connections when no
    /// <see cref="IDbConnectionFactory"/> is registered for this data source or its provider.
    /// </summary>
    /// <remarks>
    /// The driver must already have called <c>DbProviderFactories.RegisterFactory</c> under this
    /// name. Registering a factory is the simpler route and does not depend on a process-wide state.
    /// </remarks>
    public string? ProviderInvariantName { get; set; }
}
