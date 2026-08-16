using System.Data;
using System.Data.Common;
using ApricotFramework.DataOps.AspNetCore.Extensions;
using ApricotFramework.DataOps.AspNetCore.Options;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApricotFramework.DataOps.AspNetCore.Tests;

public class ConnectionResolutionTests
{
    [Fact]
    public void CreateOrNull_UnknownDataSource_ReturnsNull()
    {
        var registry = Registry(services => services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs)));

        Assert.Null(registry.CreateOrNull("Absent"));
    }

    /// <summary>
    /// The documented behaviour is that a data source name matches case-insensitively, which
    /// survives configuration binding only because the bound dictionary keeps its comparer.
    /// </summary>
    [Theory]
    [InlineData("Main")]
    [InlineData("main")]
    [InlineData("MAIN")]
    public void CreateOrNull_DataSourceName_MatchesCaseInsensitively(string name)
    {
        var services = new ServiceCollection();
        services.AddDataOperations(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DataOperations:DefaultDataSource"] = "Main",
            ["DataOperations:DataSources:Main:Provider"] = "Sqlite",
            ["DataOperations:DataSources:Main:ConnectionString"] = "Data Source=:memory:"
        }).Build());
        services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs));

        var registry = services.BuildServiceProvider(validateScopes: true).GetRequiredService<IConnectionRegistry>();

        using var reference = registry.CreateOrNull(name);

        Assert.NotNull(reference);
    }

    [Fact]
    public void CreateOrNull_ProviderFactory_BuildsTheConnection()
    {
        var registry = Registry(services => services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs)));

        using var reference = registry.CreateOrNull("Main")!;

        Assert.IsType<SqliteConnection>(reference.Connection);
        Assert.Equal(SqlProvider.Sqlite, reference.Provider);
        Assert.Equal(ConnectionState.Closed, reference.Connection.State);
    }

    /// <summary>
    /// A factory keyed to the data source name has to beat the one registered for its provider,
    /// which is the only way two sources on one provider can use different drivers.
    /// </summary>
    [Fact]
    public void CreateOrNull_NameKeyedFactory_BeatsTheProviderFactory()
    {
        var registry = Registry(services =>
        {
            services.AddDataSourceConnection(SqlProvider.Sqlite, _ => new SqliteConnection("Data Source=:memory:"));
            services.AddDataSourceConnection("Main", SqlProvider.Sqlite, _ => new MarkerConnection());
        });

        using var reference = registry.CreateOrNull("Main")!;

        Assert.IsType<MarkerConnection>(reference.Connection);
    }

    [Fact]
    public void CreateOrNull_NoFactoryAndNoInvariantName_SaysHowToFixIt()
    {
        var registry = Registry(_ => { });

        var exception = Assert.Throws<InvalidOperationException>(() => registry.CreateOrNull("Main"));

        Assert.Contains("AddDataSourceConnection", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ProviderInvariantName", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateOrNull_InvariantNameFallback_UsesTheRegisteredProviderFactory()
    {
        DbProviderFactories.RegisterFactory("Test.Sqlite", SqliteFactory.Instance);

        var registry = Registry(_ => { }, source => source.ProviderInvariantName = "Test.Sqlite");

        using var reference = registry.CreateOrNull("Main")!;

        Assert.IsType<SqliteConnection>(reference.Connection);
        Assert.Equal("Data Source=:memory:", reference.Connection.ConnectionString);
    }

    [Fact]
    public void CreateOrNull_DataSourceWithoutAProvider_SaysSo()
    {
        var registry = Registry(_ => { }, source => source.Provider = null);

        var exception = Assert.Throws<InvalidOperationException>(() => registry.CreateOrNull("Main"));

        Assert.Contains("does not declare a provider", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetDefaultDataSource_NoneConfigured_SaysSo()
    {
        var services = new ServiceCollection();
        services.AddDataOperations(options => options.DataSources["Main"] = new DataSourceOptions());

        var registry = services.BuildServiceProvider(validateScopes: true).GetRequiredService<IConnectionRegistry>();

        Assert.Throws<InvalidOperationException>(registry.GetDefaultDataSource);
    }

    [Fact]
    public void CreateOrNull_OwningReference_DisposesTheConnectionItCreated()
    {
        var registry = Registry(services => services.AddDataSourceConnection(SqlProvider.Sqlite, _ => new MarkerConnection()));

        var reference = registry.CreateOrNull("Main")!;
        var connection = (MarkerConnection)reference.Connection;

        reference.Dispose();

        Assert.True(connection.Disposed);
    }

    private static IConnectionRegistry Registry(Action<IServiceCollection> configureFactories, Action<DataSourceOptions>? configureSource = null)
    {
        var services = new ServiceCollection();

        services.AddDataOperations(options =>
        {
            options.DefaultDataSource = "Main";

            var source = new DataSourceOptions { Provider = SqlProvider.Sqlite, ConnectionString = "Data Source=:memory:" };
            configureSource?.Invoke(source);

            options.DataSources["Main"] = source;
        });

        configureFactories(services);

        return services.BuildServiceProvider(validateScopes: true).GetRequiredService<IConnectionRegistry>();
    }

    /// <summary>
    /// A connection that only records that it was disposed, so ownership can be observed.
    /// </summary>
    private sealed class MarkerConnection : IDbConnection
    {
        public bool Disposed { get; private set; }

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public string ConnectionString { get; set; } = string.Empty;

        public int ConnectionTimeout => 0;

        public string Database => string.Empty;

        public ConnectionState State => ConnectionState.Closed;

        public IDbTransaction BeginTransaction() => throw new NotSupportedException();

        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotSupportedException();

        public void ChangeDatabase(string databaseName) => throw new NotSupportedException();

        public void Close()
        {
        }

        public IDbCommand CreateCommand() => throw new NotSupportedException();

        public void Open() => throw new NotSupportedException();

        public void Dispose() => this.Disposed = true;
    }
}
