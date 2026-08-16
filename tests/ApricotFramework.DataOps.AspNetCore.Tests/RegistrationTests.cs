using ApricotFramework.DataOps.AspNetCore.Extensions;
using ApricotFramework.DataOps.AspNetCore.Options;
using ApricotFramework.DataOps.Definitions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApricotFramework.DataOps.AspNetCore.Tests;

public class RegistrationTests
{
    [Fact]
    public void AddDataOperations_BindsDataSourcesFromConfiguration()
    {
        var provider = Build(Configuration());

        var options = provider.GetRequiredService<IOptions<DataOperationsOptions>>().Value;

        Assert.Equal("Main", options.DefaultDataSource);
        Assert.Equal(SqlProvider.Sqlite, options.DataSources["Main"].Provider);
        Assert.Equal("Data Source=:memory:", options.DataSources["Main"].ConnectionString);
    }

    [Fact]
    public void AddDataOperations_ResolvesTheEntryPointUnderScopeValidation()
    {
        var provider = Build(Configuration());

        Assert.NotNull(provider.GetRequiredService<IDataOperations>());
        Assert.NotNull(provider.GetRequiredService<IOperationRegistry>());
        Assert.NotNull(provider.GetRequiredService<IConnectionRegistry>());
    }

    [Fact]
    public void AddDataOperations_ConfiguredInCode_NeedsNoConfigurationSection()
    {
        var services = new ServiceCollection();

        services.AddDataOperations(options =>
        {
            options.DefaultDataSource = "Main";
            options.DataSources["Main"] = new DataSourceOptions { Provider = SqlProvider.Sqlite, ConnectionString = "Data Source=:memory:" };
        });

        services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs));

        var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Equal("Main", provider.GetRequiredService<IConnectionRegistry>().GetDefaultDataSource());
    }

    [Fact]
    public void AddConnectionRegistry_RegisteredFirst_ReplacesTheConfigurationAwareOne()
    {
        var services = new ServiceCollection();

        services.AddConnectionRegistry(new StubConnectionRegistry());
        services.AddDataOperations(Configuration());

        var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<StubConnectionRegistry>(provider.GetRequiredService<IConnectionRegistry>());
    }

    [Fact]
    public void AddOperationsDefinitionSource_IsAdditive()
    {
        var services = new ServiceCollection();

        services.AddOperationsDefinitionSource(new StubDefinitionSource());
        services.AddOperationsDefinitionSource(new StubDefinitionSource());
        services.AddDataOperations(Configuration());

        var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Equal(2, provider.GetServices<IOperationsDefinitionSource>().Count());
    }

    private static IConfiguration Configuration(Dictionary<string, string?>? extra = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["DataOperations:DefaultDataSource"] = "Main",
            ["DataOperations:DataSources:Main:Provider"] = "Sqlite",
            ["DataOperations:DataSources:Main:ConnectionString"] = "Data Source=:memory:"
        };

        foreach (var (key, value) in extra ?? [])
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static ServiceProvider Build(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddDataOperations(configuration);
        services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs));
        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class StubConnectionRegistry : IConnectionRegistry
    {
        public string GetDefaultDataSource() => "stub";

        public IConnectionReference? CreateOrNull(string? dataSource) => null;
    }

    private sealed class StubDefinitionSource : IOperationsDefinitionSource
    {
        public IEnumerable<DataOperationsDefinition> GetAll() => [];
    }
}
