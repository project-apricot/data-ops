using System.Data;
using Microsoft.Data.Sqlite;

namespace ApricotFramework.DataOps.Tests;

/// <summary>
/// What the library hands to ADO.NET: the command type, the command text, the timeout and the
/// isolation level. These are the parts a second database engine would exercise but not change,
/// because the library selects authored SQL rather than generating any.
/// </summary>
public class AdoContractTests
{
    private const string Definitions = """
        <DataOperations>
          <OperationGroup Name="Ado" Compatibility="Sqlite">
            <SqlOperation Name="Text">
              <TextCommand ExpectedResult="Scalar">SELECT 1</TextCommand>
            </SqlOperation>
            <SqlOperation Name="Procedure">
              <StoredProcedure Name="sp_create_person" ExpectedResult="RowCount" />
            </SqlOperation>
            <SqlOperation Name="Timed" Timeout="PT90S">
              <TextCommand ExpectedResult="Scalar">SELECT 1</TextCommand>
            </SqlOperation>
            <SqlOperation Name="Untimed">
              <TextCommand ExpectedResult="Scalar">SELECT 1</TextCommand>
            </SqlOperation>
            <SqlOperation Name="Conditional">
              <TextCommand ExpectedResult="Scalar">SELECT {if:one{ 1 } else { 2 }}</TextCommand>
            </SqlOperation>
          </OperationGroup>
        </DataOperations>
        """;

    /// <summary>
    /// SQLite has no stored procedures and its command object rejects the command type outright, so
    /// this is the only place the declaration is proven to reach ADO.NET at all.
    /// </summary>
    [Fact]
    public async Task StoredProcedure_AsksForTheStoredProcedureCommandType()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().NonQuery("Ado", "Procedure").ExecuteAsync();

        Assert.Equal(CommandType.StoredProcedure, harness.Recorder.LastCommand.CommandType);
    }

    [Fact]
    public async Task StoredProcedure_PassesTheProcedureNameAsTheCommandText()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().NonQuery("Ado", "Procedure").ExecuteAsync();

        Assert.Equal("sp_create_person", harness.Recorder.LastCommand.CommandText);
    }

    [Fact]
    public async Task TextCommand_AsksForTheTextCommandType()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Text").ExecuteAsync<long>();

        Assert.Equal(CommandType.Text, harness.Recorder.LastCommand.CommandType);
    }

    [Fact]
    public async Task Timeout_IsPassedInWholeSeconds()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Timed").ExecuteAsync<long>();

        Assert.Equal(90, harness.Recorder.LastCommand.CommandTimeout);
    }

    [Fact]
    public async Task WithTimeout_OverridesWhatTheDefinitionDeclared()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Timed")
            .WithTimeout(TimeSpan.FromSeconds(5))
            .ExecuteAsync<long>();

        Assert.Equal(5, harness.Recorder.LastCommand.CommandTimeout);
    }

    [Fact]
    public async Task Timeout_Undeclared_LeavesTheDriverDefault()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Untimed").ExecuteAsync<long>();

        using var untouched = new SqliteCommand();
        Assert.Equal(untouched.CommandTimeout, harness.Recorder.LastCommand.CommandTimeout);
    }

    /// <summary>
    /// SQLite coerces every level to Serializable, so the requested level is only observable on the
    /// way in. On an engine that distinguishes them, this is what it would receive.
    /// </summary>
    [Theory]
    [InlineData(AutoTransaction.ReadUncommitted, IsolationLevel.ReadUncommitted)]
    [InlineData(AutoTransaction.ReadCommitted, IsolationLevel.ReadCommitted)]
    [InlineData(AutoTransaction.RepeatableRead, IsolationLevel.RepeatableRead)]
    [InlineData(AutoTransaction.Serializable, IsolationLevel.Serializable)]
    public async Task AutoTransaction_MapsOntoTheAdoIsolationLevel(AutoTransaction declared, IsolationLevel expected)
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Text")
            .WithTransaction(declared)
            .ExecuteAsync<long>();

        Assert.Equal(expected, Assert.Single(harness.Recorder.Transactions));
    }

    [Theory]
    [InlineData(AutoTransaction.ReadCommitted, IsolationLevel.ReadCommitted)]
    [InlineData(AutoTransaction.Serializable, IsolationLevel.Serializable)]
    public async Task BeginAsync_OpensAtTheRequestedIsolationLevel(AutoTransaction declared, IsolationLevel expected)
    {
        using var harness = SqliteHarness.Create(Definitions);

        await using var scope = await harness.Operations.BeginAsync(declared, TestContext.Current.CancellationToken);

        Assert.Equal(expected, Assert.Single(harness.Recorder.Transactions));
    }

    [Fact]
    public async Task AutoTransactionNo_OpensNoTransactionAtAll()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Text")
            .WithTransaction(AutoTransaction.No)
            .ExecuteAsync<long>();

        Assert.Empty(harness.Recorder.Transactions);
    }

    [Fact]
    public async Task Undeclared_OpensNoTransactionAtAll()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Text").ExecuteAsync<long>();

        Assert.Empty(harness.Recorder.Transactions);
    }

    [Fact]
    public async Task BeginAsync_OpensOneTransactionForEveryOperationInIt()
    {
        using var harness = SqliteHarness.Create(Definitions);

        await using var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken);
        await scope.Router.Scalar("Ado", "Text").ExecuteAsync<long>();
        await scope.Router.Scalar("Ado", "Text").ExecuteAsync<long>();

        Assert.Single(harness.Recorder.Transactions);
        Assert.Equal(2, harness.Recorder.Commands.Count);
    }

    [Theory]
    [InlineData(true, "SELECT 1")]
    [InlineData(false, "SELECT 2")]
    public async Task ConditionalFragment_IsResolvedBeforeTheCommandTextIsSent(bool one, string expected)
    {
        using var harness = SqliteHarness.Create(Definitions);

        await harness.Operations.Connect().Scalar("Ado", "Conditional")
            .WithBinding("one", one)
            .ExecuteAsync<long>();

        Assert.Equal(expected, harness.Recorder.LastCommand.CommandText);
    }
}
