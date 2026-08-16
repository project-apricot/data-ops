using Microsoft.Data.Sqlite;

namespace ApricotFramework.DataOps.Tests;

public class TransactionTests
{
    [Fact]
    public async Task AutoTransaction_OperationSucceeds_Commits()
    {
        using var harness = SqliteHarness.Create(Operations.PeopleTransactional);

        await harness.Operations.Connect().NonQuery("Txn", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });

        Assert.Equal(1, harness.CountPeople());
    }

    [Fact]
    public async Task AutoTransaction_OperationThrows_RollsBack()
    {
        using var harness = SqliteHarness.Create(Operations.PeopleTransactional);

        await Assert.ThrowsAsync<SqliteException>(() => harness.Operations.Connect()
            .NonQuery("Txn", "Fail")
            .ExecuteAsync(new { Age = 36 }));

        Assert.Equal(0, harness.CountPeople());
    }

    [Fact]
    public async Task AutoTransactionNo_OverridesTheInheritedTransaction()
    {
        using var harness = SqliteHarness.Create(Operations.PeopleTransactional);

        var executor = harness.Operations.Connect().NonQuery("Txn", "InsertNoTransaction");

        Assert.Null(executor.AutoTransaction);
        Assert.False(executor.AutoCommit);

        await executor.ExecuteAsync(new { FirstName = "Ada", Age = 36 });

        Assert.Equal(1, harness.CountPeople());
    }

    [Fact]
    public async Task BeginAsync_Commit_PersistsEveryOperation()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        await using (var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken))
        {
            await scope.Router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });
            await scope.Router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Grace", Age = 45 });
            await scope.CommitAsync();
        }

        Assert.Equal(2, harness.CountPeople());
    }

    [Fact]
    public async Task BeginAsync_DisposedWithoutCommit_RollsBack()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        await using (var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken))
        {
            await scope.Router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });
        }

        Assert.Equal(0, harness.CountPeople());
    }

    [Fact]
    public async Task BeginAsync_OperationThrows_LeavesNothingBehind()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        await Assert.ThrowsAsync<SqliteException>(async () =>
        {
            await using var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken);
            await scope.Router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });
            await scope.Router.NonQuery("People", "Fail").ExecuteAsync(new { Id = 1, Age = 1 });
            await scope.CommitAsync();
        });

        Assert.Equal(0, harness.CountPeople());
    }

    [Fact]
    public async Task BeginAsync_OperationDeclaringItsOwnTransaction_JoinsTheScopeInstead()
    {
        using var harness = SqliteHarness.Create(Operations.PeopleTransactional);

        await using var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken);

        var executor = scope.Router.NonQuery("Txn", "Insert");

        Assert.NotNull(executor.Transaction);
        Assert.Null(executor.AutoTransaction);
        Assert.False(executor.AutoCommit);

        await executor.ExecuteAsync(new { FirstName = "Ada", Age = 36 });

        // Still uncommitted, so a reader outside the scope sees nothing.
        Assert.Equal(0, harness.CountPeople());
    }

    [Fact]
    public async Task BeginAsync_ReadsBackItsOwnUncommittedWrites()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        await using var scope = await harness.Operations.BeginAsync(AutoTransaction.Serializable, TestContext.Current.CancellationToken);

        await scope.Router.NonQuery("People", "Insert").ExecuteAsync(new { FirstName = "Ada", Age = 36 });

        var count = await scope.Router.Scalar("People", "Count").ExecuteAsync<long>();

        Assert.Equal(1, count);
    }
}
