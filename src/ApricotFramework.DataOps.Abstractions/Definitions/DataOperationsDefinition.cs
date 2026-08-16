namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// Everything one definition source contributes: file-wide settings and the groups under them.
/// </summary>
public class DataOperationsDefinition
{
    /// <summary>
    /// Gets or sets the settings the groups inherit from, or null when the file declares none.
    /// </summary>
    public DataConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets the groups declared in this definition.
    /// </summary>
    public IList<OperationGroup> Groups { get; } = [];
}
