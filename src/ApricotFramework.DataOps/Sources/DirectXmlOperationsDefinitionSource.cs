using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Sources;

/// <summary>
/// Contributes definitions read from files on disk, which can be edited without a rebuild.
/// </summary>
public class DirectXmlOperationsDefinitionSource : XmlOperationsDefinitionSource
{
    /// <summary>
    /// Creates a source over the matching files under a directory.
    /// </summary>
    /// <param name="options">Where and what to match.</param>
    /// <param name="validate">Whether to check each document against the published schema.</param>
    public DirectXmlOperationsDefinitionSource(FilesFinderOptions options, bool validate = true)
        : base(SourceFinder.FindFiles(options), validate)
    {
    }
}
