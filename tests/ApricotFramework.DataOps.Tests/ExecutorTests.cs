using Microsoft.Data.Sqlite;

namespace ApricotFramework.DataOps.Tests;

public class ExecutorTests
{
    public ExecutorTests()
    {
        DapperTypeMapping.SetMatchNamesWithUnderscores(true);
    }

    [Fact]
    public async Task NonQuery_Insert_ReturnsAffectedRows()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        var affected = await harness.Operations.Connect()
            .NonQuery("People", "Insert")
            .ExecuteAsync(new { FirstName = "Ada", Age = 36 });

        Assert.Equal(1, affected);
        Assert.Equal(1, harness.CountPeople());
    }

    [Fact]
    public async Task Query_MapsUnderscoreColumnsToProperties()
    {
        using var harness = SqliteHarness.Create(Operations.People);
        await Insert(harness, "Ada", 36);

        var people = await harness.Operations.Connect().Query("People", "All").ExecuteAsync<Person>();

        var person = Assert.Single(people);
        Assert.Equal("Ada", person.FirstName);
        Assert.Equal(36, person.Age);
    }

    [Fact]
    public async Task QueryFirst_NoRows_ReturnsDefault()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        var person = await harness.Operations.Connect()
            .QueryFirst("People", "ById")
            .ExecuteAsync<Person>(new { Id = 404 });

        Assert.Null(person);
    }

    [Fact]
    public async Task QueryFirst_SingleRowMode_NoRows_Throws()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Operations.Connect()
            .QueryFirst("People", "ById")
            .ExecuteAsync<Person>(new { Id = 404 }, QueryFirstMode.SingleRow));
    }

    [Fact]
    public async Task Scalar_ReturnsSingleValue()
    {
        using var harness = SqliteHarness.Create(Operations.People);
        await Insert(harness, "Ada", 36);
        await Insert(harness, "Grace", 45);

        var count = await harness.Operations.Connect().Scalar("People", "Count").ExecuteAsync<long>();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task MultiQuery_ReadsEverySetInOrder()
    {
        using var harness = SqliteHarness.Create(Operations.People);
        await Insert(harness, "Ada", 36);
        await Insert(harness, "Grace", 45);

        var result = await harness.Operations.Connect()
            .MultiQuery("People", "AllAndCount")
            .ExecuteAsync(async reader =>
            {
                var people = await reader.ReadAsync<Person>();
                var count = await reader.ReadFirstOrDefaultAsync<long>();
                return (People: people.ToList(), Count: count);
            });

        Assert.Equal(2, result.People.Count);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Query_WrongExecutorForDeclaredResult_Throws()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        var exception = Assert.Throws<IncorrectOperationException>(() => harness.Operations.Connect().Query("People", "Count"));

        Assert.Contains("People/Count", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Query_UndefinedOperation_Throws()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        var exception = Assert.Throws<IncorrectOperationException>(() => harness.Operations.Connect().Query("People", "Missing"));

        Assert.Contains("not defined", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Query_OperationNotCompatibleWithProvider_Throws()
    {
        const string mySqlOnly = """
            <?xml version="1.0" encoding="utf-8" ?>
            <DataOperations>
              <OperationGroup Name="People" Compatibility="MySql">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """;

        using var harness = SqliteHarness.Create(mySqlOnly);

        var exception = Assert.Throws<IncorrectOperationException>(() => harness.Operations.Connect().Query("People", "All"));

        Assert.Contains("not supported for provider Sqlite", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, "Grace")]
    [InlineData(false, "Ada")]
    public async Task Query_BindingSelectsTheAuthoredFragment(bool byAge, string expectedFirst)
    {
        using var harness = SqliteHarness.Create(Operations.People);
        await Insert(harness, "Grace", 45);
        await Insert(harness, "Ada", 36);

        var people = await harness.Operations.Connect()
            .Query("People", "Sorted")
            .WithBinding("byAge", byAge)
            .ExecuteAsync<Person>();

        Assert.Equal(expectedFirst, people.First().FirstName);
    }

    [Fact]
    public async Task Query_UnknownBinding_TakesTheElseBranch()
    {
        using var harness = SqliteHarness.Create(Operations.People);
        await Insert(harness, "Grace", 45);
        await Insert(harness, "Ada", 36);

        var people = await harness.Operations.Connect().Query("People", "Sorted").ExecuteAsync<Person>();

        Assert.Equal("Ada", people.First().FirstName);
    }

    [Fact]
    public async Task Connect_CallerSuppliedConnection_IsLeftOpenAndUndisposed()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        using var connection = new SqliteConnection(harness.ConnectionString);
        connection.Open();

        var router = harness.Operations.Connect(connection, SqlProvider.Sqlite);
        await router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });
        router.Dispose();

        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task Connect_CallerSuppliedTransaction_SpansSeveralOperations()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        using var connection = new SqliteConnection(harness.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var router = harness.Operations.Connect(connection, SqlProvider.Sqlite);
        await router.NonQuery("People", "Insert").WithTransaction(transaction).ExecuteAsync(new { FirstName = "Ada", Age = 36 });
        await router.NonQuery("People", "Insert").WithTransaction(transaction).ExecuteAsync(new { FirstName = "Grace", Age = 45 });

        transaction.Rollback();

        Assert.Equal(0, harness.CountPeople());
    }

    private static Task<int> Insert(SqliteHarness harness, string firstName, int age)
    {
        return harness.Operations.Connect().NonQuery("People", "Insert").ExecuteAsync(new { FirstName = firstName, Age = age });
    }
}
