using System.Reflection;
using System.Resources;
using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Tests;

public class SourceFinderTests
{
    [Fact]
    public void FindFiles_MatchingSuffix_FindsThem()
    {
        using var directory = new TempDirectory();
        directory.Write("a.xml", "<DataOperations />");
        directory.Write("b.xml", "<DataOperations />");
        directory.Write("notes.txt", "ignored");

        var found = SourceFinder.FindFiles(new FilesFinderOptions { WorkingDirectory = directory.Path });

        Assert.Equal(2, found.Count());
    }

    [Fact]
    public void FindFiles_NotRecursive_IgnoresNestedDirectories()
    {
        using var directory = new TempDirectory();
        directory.Write("a.xml", "<DataOperations />");
        directory.Write(Path.Combine("nested", "b.xml"), "<DataOperations />");

        var flat = SourceFinder.FindFiles(new FilesFinderOptions { WorkingDirectory = directory.Path });
        var deep = SourceFinder.FindFiles(new FilesFinderOptions { WorkingDirectory = directory.Path, Recursive = true });

        Assert.Single(flat);
        Assert.Equal(2, deep.Count());
    }

    [Fact]
    public void FindFiles_MissingDirectory_Throws()
    {
        var options = new FilesFinderOptions { WorkingDirectory = Path.Combine(Path.GetTempPath(), $"absent-{Guid.NewGuid():N}") };

        Assert.Throws<DirectoryNotFoundException>(() => SourceFinder.FindFiles(options));
    }

    /// <summary>
    /// Each result opens its own stream, so enumerating the same source twice does not hand out a
    /// stream the first pass already closed.
    /// </summary>
    [Fact]
    public void FindFiles_EnumeratedTwice_OpensAFreshStreamEachTime()
    {
        using var directory = new TempDirectory();
        directory.Write("a.xml", "<DataOperations />");

        var suppliers = SourceFinder.FindFiles(new FilesFinderOptions { WorkingDirectory = directory.Path }).ToList();

        foreach (var _ in Enumerable.Range(0, 2))
        {
            using var stream = suppliers[0].Invoke();
            Assert.True(stream.CanRead);
        }
    }

    [Fact]
    public void FindEmbedded_MissingLogicalName_ThrowsWhenOpened()
    {
        var supplier = SourceFinder.FindEmbedded(Assembly.GetExecutingAssembly(), "Nope.xml").Single();

        Assert.Throws<MissingManifestResourceException>(() => supplier.Invoke());
    }

    [Fact]
    public void FindEmbedded_NoMatchingResources_FindsNothing()
    {
        var options = new EmbeddedFinderOptions { DefaultNamespace = "Nothing.Here" };

        Assert.Empty(SourceFinder.FindEmbedded(Assembly.GetExecutingAssembly(), options));
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dataops-{Guid.NewGuid():N}");
            Directory.CreateDirectory(this.Path);
        }

        public string Path { get; }

        public void Write(string relativePath, string content)
        {
            var full = System.IO.Path.Combine(this.Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
            File.WriteAllText(full, content);
        }

        public void Dispose()
        {
            Directory.Delete(this.Path, recursive: true);
        }
    }
}
