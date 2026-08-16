namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Selects which files on disk hold operations definitions.
/// </summary>
public class FilesFinderOptions
{
    /// <summary>
    /// Gets or sets the root to search under, defaulting to the directory the assembly loaded from.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Gets the folders below the root to search in.
    /// </summary>
    public IList<string> Directories { get; } = [];

    /// <summary>
    /// Gets or sets whether to search nested folders.
    /// </summary>
    public bool Recursive { get; set; }

    /// <summary>
    /// Gets or sets the file name suffix to match; null means <c>.xml</c>.
    /// </summary>
    public string? Postfix { get; set; }
}
