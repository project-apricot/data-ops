using System.Data;
using System.Data.Common;
using ApricotFramework.DataOps.AspNetCore.Options;
using ApricotFramework.DataOps.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApricotFramework.DataOps.AspNetCore.Impl;

/// <summary>
/// Resolves data sources declared in configuration into connections.
/// </summary>
/// <remarks>
/// A factory keyed to the data source name wins, then one registered for its provider, and only
/// then the ADO.NET invariant name the data source declares.
/// </remarks>
public class ConfigAwareConnectionRegistry : IConnectionRegistry
{
    private readonly IOptionsMonitor<DataOperationsOptions> optionsMonitor;
    private readonly IServiceProvider services;
    private readonly Dictionary<SqlProvider, IDbConnectionFactory> factoriesByProvider;

    /// <summary>
    /// Creates a registry over the configured data sources and the registered factories.
    /// </summary>
    /// <param name="optionsMonitor">The configured data sources.</param>
    /// <param name="factories">The factories registered for a provider.</param>
    /// <param name="services">Resolves factories keyed to a data source name.</param>
    public ConfigAwareConnectionRegistry(
        IOptionsMonitor<DataOperationsOptions> optionsMonitor,
        IEnumerable<IDbConnectionFactory> factories,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(factories);

        this.optionsMonitor = optionsMonitor;
        this.services = services;

        // A later registration for the same provider replaces an earlier one.
        this.factoriesByProvider = factories.ToDictionary(factory => factory.GetProvider(), factory => factory);
    }

    /// <inheritdoc />
    public virtual string GetDefaultDataSource()
    {
        return this.optionsMonitor.CurrentValue.DefaultDataSource
            ?? throw new InvalidOperationException($"No default data source is configured. Set {DataOperationsOptions.SectionName}:DefaultDataSource, or name a data source when connecting.");
    }

    /// <inheritdoc />
    public virtual IConnectionReference? CreateOrNull(string? dataSource)
    {
        var name = dataSource ?? this.GetDefaultDataSource();

        if (!this.optionsMonitor.CurrentValue.DataSources.TryGetValue(name, out var settings))
        {
            return null;
        }

        if (!settings.Provider.HasValue)
        {
            throw new InvalidOperationException($"The data source '{name}' does not declare a provider.");
        }

        var connection = this.CreateConnection(name, settings, settings.Provider.Value);

        return ConnectionReference.Owning(connection, settings.Provider.Value);
    }

    /// <summary>
    /// Builds a closed connection for a data source.
    /// </summary>
    /// <param name="name">The data source name.</param>
    /// <param name="settings">The configured data source.</param>
    /// <param name="provider">The provider it declares.</param>
    /// <returns>The connection.</returns>
    protected virtual IDbConnection CreateConnection(string name, DataSourceOptions settings, SqlProvider provider)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var connectionString = settings.ConnectionString
            ?? throw new InvalidOperationException($"The data source '{name}' does not declare a connection string.");

        var factory = this.services.GetKeyedService<IDbConnectionFactory>(name)
            ?? this.factoriesByProvider.GetValueOrDefault(provider);

        if (factory is not null)
        {
            return factory.CreateConnection(connectionString);
        }

        return CreateFromProviderFactory(name, settings, connectionString);
    }

    private static DbConnection CreateFromProviderFactory(string name, DataSourceOptions settings, string connectionString)
    {
        var invariantName = settings.ProviderInvariantName
            ?? throw new InvalidOperationException(
                $"The data source '{name}' has no connection factory. Register one with AddDataSourceConnection, or set its ProviderInvariantName to a driver already registered with DbProviderFactories.");

        var connection = DbProviderFactories.GetFactory(invariantName).CreateConnection()
            ?? throw new InvalidOperationException($"The '{invariantName}' provider factory did not create a connection for data source '{name}'.");

        connection.ConnectionString = connectionString;

        return connection;
    }
}
