using System.Text;
using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Tests;

/// <summary>
/// Definition documents are authored by hand, so a mistake in one must produce a message that names
/// the problem rather than an opaque failure from somewhere deeper.
/// </summary>
public class AdversarialInputTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not xml at all")]
    [InlineData("<DataOperations>")]
    [InlineData("<DataOperations><OperationGroup Name=\"x\"></DataOperations>")]
    public void Parse_MalformedDocument_ThrowsParsingException(string xml)
    {
        Assert.Throws<OperationParsingException>(() => Parse(xml));
    }

    [Fact]
    public void Parse_OperationWithNoCommand_NamesTheOperation()
    {
        var exception = Assert.Throws<OperationParsingException>(() => Parse("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="Empty" />
              </OperationGroup>
            </DataOperations>
            """, validate: false));

        Assert.Contains("People/Empty", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_EmptyDocument_YieldsNoGroups()
    {
        var definition = Parse("<DataOperations />");

        Assert.Empty(definition.Groups);
    }

    [Fact]
    public void Parse_GroupWithNoOperations_YieldsAnEmptyGroup()
    {
        var definition = Parse("""
            <DataOperations>
              <OperationGroup Name="People" />
            </DataOperations>
            """);

        Assert.Empty(definition.Groups.Single().Operations);
    }

    /// <summary>
    /// A DTD is refused outright, before any entity is resolved, so entity expansion cannot be used
    /// to blow up the parse.
    /// </summary>
    [Fact]
    public void Parse_InternalDtd_IsRefused()
    {
        Assert.Throws<OperationParsingException>(() => Parse("""
            <?xml version="1.0"?>
            <!DOCTYPE DataOperations [<!ENTITY greeting "hello">]>
            <DataOperations>
              <OperationGroup Name="&greeting;" />
            </DataOperations>
            """, validate: false));
    }

    /// <summary>
    /// A definition file must not be able to reach the filesystem or the network.
    /// </summary>
    [Fact]
    public void Parse_ExternalEntity_IsRefused()
    {
        Assert.Throws<OperationParsingException>(() => Parse("""
            <?xml version="1.0"?>
            <!DOCTYPE DataOperations [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
            <DataOperations>
              <OperationGroup Name="&xxe;" />
            </DataOperations>
            """, validate: false));
    }

    [Fact]
    public void Parse_UnparseableTimeout_IsIgnoredRatherThanFatal()
    {
        var definition = Parse("""
            <DataOperations>
              <DataConfiguration Timeout="whenever" />
              <OperationGroup Name="People" />
            </DataOperations>
            """, validate: false);

        Assert.Null(definition.Configuration!.Timeout);
    }

    [Fact]
    public void Parse_NumericEnumValue_IsRefused()
    {
        var definition = Parse("""
            <DataOperations>
              <OperationGroup Name="People">
                <SqlOperation Name="All">
                  <TextCommand ExpectedResult="1">SELECT 1</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """, validate: false);

        Assert.Equal(ResultType.Unknown, definition.Groups.Single().Operations.Single().Command!.ExpectedResult);
    }

    [Fact]
    public async Task Query_NonAsciiGroupAndOperationNames_Resolve()
    {
        using var harness = SqliteHarness.Create("""
            <DataOperations>
              <OperationGroup Name="Ünïcode">
                <SqlOperation Name="日本語">
                  <TextCommand ExpectedResult="Scalar">SELECT 42</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        var value = await harness.Operations.Connect().Scalar("Ünïcode", "日本語").ExecuteAsync<long>();

        Assert.Equal(42, value);
    }

    /// <summary>
    /// Braces appear in SQL for JSON and for string literals, and must survive the templating pass
    /// untouched when they are not a conditional fragment.
    /// </summary>
    [Fact]
    public async Task Query_BracesThatAreNotAFragment_AreLeftAlone()
    {
        using var harness = SqliteHarness.Create("""
            <DataOperations>
              <OperationGroup Name="Braces">
                <SqlOperation Name="Literal">
                  <TextCommand ExpectedResult="Scalar">SELECT '{"a": 1}'</TextCommand>
                </SqlOperation>
              </OperationGroup>
            </DataOperations>
            """);

        var value = await harness.Operations.Connect().Scalar("Braces", "Literal").ExecuteAsync<string>();

        Assert.Equal("""{"a": 1}""", value);
    }

    [Fact]
    public void Connect_UnknownDataSource_NamesIt()
    {
        using var harness = SqliteHarness.Create(Operations.People);

        var exception = Assert.Throws<IncorrectOperationException>(() => harness.Operations.Connect("Absent"));

        Assert.Contains("Absent", exception.Message, StringComparison.Ordinal);
    }

    private static Definitions.DataOperationsDefinition Parse(string xml, bool validate = true)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return new DataOperationsParser(validate).Parse(stream);
    }
}
