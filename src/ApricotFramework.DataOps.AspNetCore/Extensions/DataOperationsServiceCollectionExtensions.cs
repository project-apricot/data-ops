using System.Data;
using ApricotFramework.DataOps.AspNetCore.Impl;
using ApricotFramework.DataOps.AspNetCore.Options;
using ApricotFramework.DataOps.Impl;
using ApricotFramework.DataOps.Initialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApricotFramework.DataOps.AspNetCore.Extensions;

/// <summary>
/// Registers data operations and the pieces a host contributes to them.
/// </summary>
public static class DataOperationsServiceCollectionExtensions
{
    /// <summary>
    /// Registers data operations against data sources declared in configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration holding the <c>DataOperations</c> section.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataOperations(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(DataOperationsOptions.SectionName);

        services.AddOptions<DataOperationsOptions>().Bind(section);

        ApplyUnderscoreMatching(section.GetValue(nameof(DataOperationsOptions.MatchNamesWithUnderscores), true));

        return services.AddDataOperationsCore();
    }

    /// <summary>
    /// Registers data operations against data sources declared in code.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Declares the data sources.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataOperations(this IServiceCollection services, Action<DataOperationsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        var probe = new DataOperationsOptions();
        configure(probe);
        ApplyUnderscoreMatching(probe.MatchNamesWithUnderscores);

        return services.AddDataOperationsCore();
    }

    /// <summary>
    /// Binds a provider to a driver, so every data source declaring that provider uses it.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="provider">The provider to bind.</param>
    /// <param name="factory">Builds a closed connection from a connection string.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataSourceConnection(this IServiceCollection services, SqlProvider provider, Func<string, IDbConnection> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        return services.AddSingleton<IDbConnectionFactory>(new DelegateDbConnectionFactory(provider, factory));
    }

    /// <summary>
    /// Binds one named data source to a driver, overriding the factory for its provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dataSource">The data source name.</param>
    /// <param name="provider">The provider the data source declares.</param>
    /// <param name="factory">Builds a closed connection from a connection string.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataSourceConnection(this IServiceCollection services, string dataSource, SqlProvider provider, Func<string, IDbConnection> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        return services.AddKeyedSingleton<IDbConnectionFactory>(dataSource, new DelegateDbConnectionFactory(provider, factory));
    }

    /// <summary>
    /// Registers a connection factory implementation for its provider.
    /// </summary>
    /// <typeparam name="TFactory">The factory type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataSourceConnection<TFactory>(this IServiceCollection services) where TFactory : class, IDbConnectionFactory
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IDbConnectionFactory, TFactory>();
    }

    /// <summary>
    /// Contributes a source of operation definitions.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddOperationsDefinitionSource<TSource>(this IServiceCollection services) where TSource : class, IOperationsDefinitionSource
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IOperationsDefinitionSource, TSource>();
    }

    /// <summary>
    /// Contributes an already-built source of operation definitions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="source">The source.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddOperationsDefinitionSource(this IServiceCollection services, IOperationsDefinitionSource source)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton(source);
    }

    /// <summary>
    /// Replaces the connection registry, taking over how data sources are resolved.
    /// </summary>
    /// <typeparam name="TRegistry">The registry type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConnectionRegistry<TRegistry>(this IServiceCollection services) where TRegistry : class, IConnectionRegistry
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IConnectionRegistry, TRegistry>();
    }

    /// <summary>
    /// Replaces the connection registry with an already-built one.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registry">The registry.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddConnectionRegistry(this IServiceCollection services, IConnectionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton(registry);
    }

    /// <summary>
    /// Replaces the operation registry, taking over how keys are resolved.
    /// </summary>
    /// <typeparam name="TRegistry">The registry type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataOperationRegistry<TRegistry>(this IServiceCollection services) where TRegistry : class, IOperationRegistry
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton<IOperationRegistry, TRegistry>();
    }

    /// <summary>
    /// Replaces the operation registry with an already-built one.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registry">The registry.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataOperationRegistry(this IServiceCollection services, IOperationRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton(registry);
    }

    /// <summary>
    /// Registers the services that do not depend on how the options were supplied. Each uses
    /// TryAdd, so a replacement registered beforehand wins.
    /// </summary>
    private static IServiceCollection AddDataOperationsCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IConnectionRegistry, ConfigAwareConnectionRegistry>();
        services.TryAddSingleton<OperationRegistryInitializer>();
        services.TryAddSingleton<IOperationRegistry, CompositeOperationRegistry>();
        services.TryAddSingleton<IDataOperations, DataOperations>();

        return services;
    }

    /// <summary>
    /// Dapper's column matching is process-wide, so it is applied here where the registration is
    /// visible rather than as a side effect of the first query.
    /// </summary>
    private static void ApplyUnderscoreMatching(bool enabled)
    {
        DapperTypeMapping.SetMatchNamesWithUnderscores(enabled);
    }
}
