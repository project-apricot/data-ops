using System.Reflection;
using System.Resources;

namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Locates the streams that hold operations definitions.
/// </summary>
/// <remarks>
/// Each result is a supplier rather than an open stream, so a source can be read more than once
/// and nothing is held open until it is needed.
/// </remarks>
public static class SourceFinder
{
    private const string DefaultPostfix = ".xml";

    /// <summary>
    /// Finds embedded resources matching a prefix and suffix.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="options">Which resources to match.</param>
    /// <returns>A supplier per matching resource.</returns>
    public static IEnumerable<Func<Stream>> FindEmbedded(Assembly assembly, EmbeddedFinderOptions options)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(options);

        var rootNamespace = string.IsNullOrWhiteSpace(options.DefaultNamespace)
            ? assembly.GetName().Name
            : options.DefaultNamespace;

        var prefix = rootNamespace ?? string.Empty;

        if (options.Directories.Count > 0)
        {
            prefix = $"{prefix}.{string.Join('.', options.Directories)}";
        }

        if (prefix.Length > 0)
        {
            prefix = $"{prefix}.";
        }

        // An empty postfix is a deliberate "match anything"; only null means "use the default".
        var postfix = options.Postfix ?? DefaultPostfix;

        var names = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(postfix, StringComparison.Ordinal));

        return names
            .Select(name => new Func<Stream>(() => OpenEmbedded(assembly, name)))
            .ToList();
    }

    /// <summary>
    /// Finds embedded resources by their exact logical names.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <param name="logicalNames">The resource names, each of which must exist.</param>
    /// <returns>A supplier per named resource.</returns>
    public static IEnumerable<Func<Stream>> FindEmbedded(Assembly assembly, params string[] logicalNames)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(logicalNames);

        return logicalNames
            .Select(name => new Func<Stream>(() => OpenEmbedded(assembly, name)))
            .ToList();
    }

    /// <summary>
    /// Finds files matching a suffix under a directory.
    /// </summary>
    /// <param name="options">Where and what to match.</param>
    /// <returns>A supplier per matching file.</returns>
    public static IEnumerable<Func<Stream>> FindFiles(FilesFinderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // The app base rather than the assembly location, which is empty in a single-file publish.
        var dirPath = string.IsNullOrWhiteSpace(options.WorkingDirectory)
            ? AppContext.BaseDirectory
            : options.WorkingDirectory;

        if (options.Directories.Count > 0)
        {
            dirPath = Path.Combine(dirPath, Path.Combine([.. options.Directories]));
        }

        var directory = new DirectoryInfo(dirPath);

        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException($"The operations directory '{directory.FullName}' does not exist.");
        }

        var postfix = options.Postfix ?? DefaultPostfix;

        var searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        return directory.GetFiles($"*{postfix}", searchOption)
            .Select(file => new Func<Stream>(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            .ToList();
    }

    private static Stream OpenEmbedded(Assembly assembly, string logicalName)
    {
        return assembly.GetManifestResourceStream(logicalName)
            ?? throw new MissingManifestResourceException($"The resource '{logicalName}' could not be found in assembly '{assembly.GetName().FullName}'.");
    }
}
