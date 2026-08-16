using System.Reflection;
using ApricotFramework.DataOps.Initialization;
using ApricotFramework.DataOps.Sources;

namespace ApricotFramework.DataOps.Examples.Web.Data;

/// <summary>
/// Operations embedded in this assembly, which is how a library ships the SQL it owns.
/// </summary>
public class EmbeddedOperationsSource : EmbeddedXmlOperationsDefinitionSource
{
    /// <summary>
    /// Creates the source over the Operations folder of this assembly.
    /// </summary>
    public EmbeddedOperationsSource()
        : base(Assembly.GetExecutingAssembly(), new EmbeddedFinderOptions { Directories = { "Operations" } })
    {
    }
}

/// <summary>
/// Operations deployed as files beside the application, which can be edited without a rebuild.
/// </summary>
public class FilesOperationsSource : DirectXmlOperationsDefinitionSource
{
    /// <summary>
    /// Creates the source over the Operations folder in the output directory.
    /// </summary>
    public FilesOperationsSource()
        : base(new FilesFinderOptions { Directories = { "Operations" } })
    {
    }
}
