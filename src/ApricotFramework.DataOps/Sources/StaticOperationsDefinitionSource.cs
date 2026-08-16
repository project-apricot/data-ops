using ApricotFramework.DataOps.Definitions;

namespace ApricotFramework.DataOps.Sources;

/// <summary>
/// Contributes definitions built in code rather than read from a document.
/// </summary>
public class StaticOperationsDefinitionSource : IOperationsDefinitionSource
{
    private readonly IReadOnlyList<DataOperationsDefinition> definitions;

    /// <summary>
    /// Creates a source over definitions the caller built.
    /// </summary>
    /// <param name="definitions">The definitions.</param>
    public StaticOperationsDefinitionSource(IReadOnlyList<DataOperationsDefinition> definitions)
    {
        this.definitions = definitions;
    }

    /// <inheritdoc />
    public IEnumerable<DataOperationsDefinition> GetAll()
    {
        return this.definitions;
    }
}
