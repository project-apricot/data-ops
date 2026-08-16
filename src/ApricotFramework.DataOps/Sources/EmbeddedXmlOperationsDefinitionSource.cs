using System.Reflection;
using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Sources;

/// <summary>
/// Contributes definitions embedded in an assembly, which is how a library ships operations it
/// owns without the host having to deploy any files.
/// </summary>
public class EmbeddedXmlOperationsDefinitionSource : XmlOperationsDefinitionSource
{
    /// <summary>
    /// Creates a source over the matching embedded resources of an assembly.
    /// </summary>
    /// <param name="assembly">The assembly holding the definitions.</param>
    /// <param name="options">Which resources to match.</param>
    /// <param name="validate">Whether to check each document against the published schema.</param>
    public EmbeddedXmlOperationsDefinitionSource(Assembly assembly, EmbeddedFinderOptions options, bool validate = true)
        : base(SourceFinder.FindEmbedded(assembly, options), validate)
    {
    }
}
