using ApricotFramework.DataOps.Definitions;
using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Sources;

/// <summary>
/// Contributes definitions read from XML documents.
/// </summary>
public class XmlOperationsDefinitionSource : IOperationsDefinitionSource
{
    private readonly IEnumerable<Func<Stream>> streamSuppliers;
    private readonly bool validate;

    /// <summary>
    /// Creates a source over documents opened on demand.
    /// </summary>
    /// <param name="streamSuppliers">Opens each document when it is read.</param>
    /// <param name="validate">Whether to check each document against the published schema.</param>
    public XmlOperationsDefinitionSource(IEnumerable<Func<Stream>> streamSuppliers, bool validate = true)
    {
        this.streamSuppliers = streamSuppliers;
        this.validate = validate;
    }

    /// <inheritdoc />
    public IEnumerable<DataOperationsDefinition> GetAll()
    {
        var parser = new DataOperationsParser(this.validate);

        return this.streamSuppliers.Select(supplier =>
        {
            using var stream = supplier.Invoke();
            return parser.Parse(stream);
        }).ToList();
    }
}
