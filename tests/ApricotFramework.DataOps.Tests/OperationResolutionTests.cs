using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Tests;

public class OperationResolutionTests
{
    /// <summary>
    /// Every provider named in the Compatibility attribute must resolve the operation, and no other
    /// provider may.
    /// </summary>
    [Theory]
    [InlineData(SqlProvider.SqlServer, true)]
    [InlineData(SqlProvider.MySql, true)]
    [InlineData(SqlProvider.PostgreSql, false)]
    [InlineData(SqlProvider.Sqlite, false)]
    public void Resolve_CompatibilityList_RegistersExactlyTheNamedProviders(SqlProvider provider, bool expected)
    {
        var operations = Resolve("""
            <DataOperations>
              <OperationGroup Name="People" Compatibility="SqlServer MySql">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        Assert.Equal(expected, operations.GetOrNull(OpKey.Of("People", "All"), provider) is not null);
    }

    [Fact]
    public void Resolve_NoCompatibility_RegistersEveryProvider()
    {
        var operations = Resolve("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        Assert.All(
            Enum.GetValues<SqlProvider>(),
            provider => Assert.NotNull(operations.GetOrNull(OpKey.Of("People", "All"), provider)));
    }

    /// <summary>
    /// File-wide compatibility has to reach the operation; the predecessor read it only from the
    /// operation, so a Compatibility on DataConfiguration silently did nothing.
    /// </summary>
    [Fact]
    public void Resolve_FileWideCompatibility_ReachesTheOperation()
    {
        var operations = Resolve("""
            <DataOperations>
              <DataConfiguration Compatibility="Sqlite" />
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        Assert.NotNull(operations.GetOrNull(OpKey.Of("People", "All"), SqlProvider.Sqlite));
        Assert.Null(operations.GetOrNull(OpKey.Of("People", "All"), SqlProvider.MySql));
    }

    [Fact]
    public void Resolve_Settings_AreOverriddenInnermostFirst()
    {
        var operations = Resolve("""
            <DataOperations>
              <DataConfiguration Timeout="PT1M" AutoTransaction="ReadUncommitted" />
              <OperationGroup Name="People" Timeout="PT2M">
                <SqlOperation Name="Inherits">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
                <SqlOperation Name="Overrides" Timeout="PT3M" AutoTransaction="Serializable">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        var inherits = operations.GetOrNull(OpKey.Of("People", "Inherits"), SqlProvider.Sqlite)!;
        var overrides = operations.GetOrNull(OpKey.Of("People", "Overrides"), SqlProvider.Sqlite)!;

        Assert.Equal(TimeSpan.FromMinutes(2), inherits.Timeout);
        Assert.Equal(AutoTransaction.ReadUncommitted, inherits.AutoTransaction);
        Assert.Equal(TimeSpan.FromMinutes(3), overrides.Timeout);
        Assert.Equal(AutoTransaction.Serializable, overrides.AutoTransaction);
    }

    [Fact]
    public void Resolve_AutoTransactionNo_ClearsTheInheritedTransaction()
    {
        var operations = Resolve("""
            <DataOperations>
              <DataConfiguration AutoTransaction="Serializable" />
              <OperationGroup Name="People">
                <SqlOperation Name="None" AutoTransaction="No">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        Assert.Null(operations.GetOrNull(OpKey.Of("People", "None"), SqlProvider.Sqlite)!.AutoTransaction);
    }

    [Fact]
    public void Resolve_SameOperationForOneProviderTwice_Throws()
    {
        const string xml = """
            <DataOperations>
              <OperationGroup Name="People" Compatibility="Sqlite">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """;

        var exception = Assert.Throws<IncorrectOperationException>(() => Resolve(xml, xml));

        Assert.Contains("People/All", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_SameOperationForDifferentProviders_Coexists()
    {
        var operations = Resolve(
            Dialect("Sqlite", "SELECT 'sqlite'"),
            Dialect("MySql", "SELECT 'mysql'"));

        Assert.Equal("SELECT 'sqlite'", operations.GetOrNull(OpKey.Of("People", "All"), SqlProvider.Sqlite)!.Source);
        Assert.Equal("SELECT 'mysql'", operations.GetOrNull(OpKey.Of("People", "All"), SqlProvider.MySql)!.Source);
    }

    private static string Dialect(string provider, string sql)
    {
        return $"""
            <DataOperations>
              <OperationGroup Name="People" Compatibility="{provider}">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">{sql}</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """;
    }

    private static Impl.CompositeOperationRegistry Resolve(params string[] documents)
    {
        var sources = documents
            .Select(xml => (IOperationsDefinitionSource)SqliteHarness.XmlSource(xml))
            .ToList();

        return new Impl.CompositeOperationRegistry(new OperationRegistryInitializer(sources));
    }
}
