using System.Text;
using ApricotFramework.DataOps.Definitions;
using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Tests;

public class DefinitionParsingTests
{
    [Fact]
    public void Parse_TextCommand_ReadsSourceTypeAndResult()
    {
        var definition = Parse("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        var operation = definition.Groups.Single().Operations.Single();

        Assert.Equal("All", operation.Name);
        Assert.Equal("SELECT 1", operation.Command!.Source);
        Assert.Equal(OperationType.Text, operation.Command.Type);
        Assert.Equal(ResultType.Table, operation.Command.ExpectedResult);
    }

    [Fact]
    public void Parse_StoredProcedure_TakesItsNameAsTheSource()
    {
        var definition = Parse("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <StoredProcedure Name="sp_all_people" ExpectedResult="Table" />
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        var command = definition.Groups.Single().Operations.Single().Command;

        Assert.Equal("sp_all_people", command!.Source);
        Assert.Equal(OperationType.StoredProcedure, command.Type);
    }

    /// <summary>
    /// The schema declares Compatibility as a whitespace-separated list, so a two-provider value has
    /// to register the operation for both rather than falling back to every provider.
    /// </summary>
    [Theory]
    [InlineData("MySql PostgreSql")]
    [InlineData("MySql,PostgreSql")]
    [InlineData("mysql postgresql")]
    public void Parse_CompatibilityList_ReadsEveryProvider(string compatibility)
    {
        var definition = Parse($"""
            <DataOperations>
              <OperationGroup Name="People" Compatibility="{compatibility}">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """, validate: false);

        Assert.Equal(
            CompatibilityProviders.MySql | CompatibilityProviders.PostgreSql,
            definition.Groups.Single().Providers);
    }

    [Fact]
    public void Parse_UnknownProvider_Throws()
    {
        var exception = Assert.Throws<OperationParsingException>(() => Parse("""
            <DataOperations>
              <OperationGroup Name="People" Compatibility="Oracle">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """, validate: false));

        Assert.Contains("Oracle", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("PT5M", 300)]
    [InlineData("00:05:00", 300)]
    [InlineData("PT1H30M", 5400)]
    public void Parse_Timeout_AcceptsDurationAndTimeSpanForms(string value, int expectedSeconds)
    {
        var definition = Parse($"""
            <DataOperations>
              <DataConfiguration Timeout="{value}" />
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """, validate: false);

        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), definition.Configuration!.Timeout);
    }

    [Fact]
    public void Parse_AutoTransactionNo_IsRecordedRatherThanTreatedAsAbsent()
    {
        var definition = Parse("""
            <DataOperations>
              <DataConfiguration AutoTransaction="Serializable" />
              <OperationGroup Name="People">
                <SqlOperation Name="All" AutoTransaction="No">
                  <TextCommand ExpectedResult="Table">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        Assert.Equal(AutoTransaction.No, definition.Groups.Single().Operations.Single().AutoTransaction);
    }

    [Fact]
    public void Parse_SchemaViolation_ReportsEveryError()
    {
        var exception = Assert.Throws<OperationParsingException>(() => Parse("""
            <DataOperations>
              <OperationGroup>
                <SqlOperation>
                  <TextCommand ExpectedResult="Nonsense">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """));

        // One pass through the validator, so the missing names and the bad enum all surface together.
        Assert.Contains("Name", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ExpectedResult", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_ValidationOff_AcceptsWhatTheSchemaWouldReject()
    {
        var definition = Parse("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="Nonsense">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """, validate: false);

        Assert.Equal(ResultType.Unknown, definition.Groups.Single().Operations.Single().Command!.ExpectedResult);
    }

    private static DataOperationsDefinition Parse(string xml, bool validate = true)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return new DataOperationsParser(validate).Parse(stream);
    }
}
