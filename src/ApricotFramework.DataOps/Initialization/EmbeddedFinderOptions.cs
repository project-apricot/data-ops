namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Selects which embedded resources of an assembly hold operations definitions.
/// </summary>
public class EmbeddedFinderOptions
{
    /// <summary>
    /// Gets or sets the resource name prefix, defaulting to the assembly name.
    /// </summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>
    /// Gets the folders below the prefix, which appear in resource names as dotted segments.
    /// </summary>
    public IList<string> Directories { get; } = [];

    /// <summary>
    /// Gets or sets the resource name suffix to match; null means <c>.xml</c>.
    /// </summary>
    public string? Postfix { get; set; }
}
