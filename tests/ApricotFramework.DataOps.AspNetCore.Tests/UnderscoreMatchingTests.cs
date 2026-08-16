using ApricotFramework.DataOps.AspNetCore.Extensions;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApricotFramework.DataOps.AspNetCore.Tests;

/// <summary>
/// These share Dapper's process-wide setting, so they live in one class to keep xunit from running
/// them alongside each other.
/// </summary>
public class UnderscoreMatchingTests : IDisposable
{
    private readonly bool original = DefaultTypeMap.MatchNamesWithUnderscores;

    [Fact]
    public void AddDataOperations_ByDefault_TurnsUnderscoreMatchingOn()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = false;

        new ServiceCollection().AddDataOperations(Configuration(null));

        Assert.True(DefaultTypeMap.MatchNamesWithUnderscores);
    }

    [Fact]
    public void AddDataOperations_TurnedOffInConfiguration_LeavesItOff()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        new ServiceCollection().AddDataOperations(Configuration("false"));

        Assert.False(DefaultTypeMap.MatchNamesWithUnderscores);
    }

    [Fact]
    public void AddDataOperations_TurnedOffInCode_LeavesItOff()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        new ServiceCollection().AddDataOperations(options => options.MatchNamesWithUnderscores = false);

        Assert.False(DefaultTypeMap.MatchNamesWithUnderscores);
    }

    public void Dispose()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = this.original;
        GC.SuppressFinalize(this);
    }

    private static IConfiguration Configuration(string? matchNamesWithUnderscores)
    {
        var values = new Dictionary<string, string?>
        {
            ["DataOperations:DefaultDataSource"] = "Main"
        };

        if (matchNamesWithUnderscores is not null)
        {
            values["DataOperations:MatchNamesWithUnderscores"] = matchNamesWithUnderscores;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
