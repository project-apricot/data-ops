using System.Text;
using ApricotFramework.DataOps.Impl;
using ApricotFramework.DataOps.Initialization;
using ApricotFramework.DataOps.Sources;
using ApricotFramework.DataOps.Tests.Recording;
using Microsoft.Data.Sqlite;

namespace ApricotFramework.DataOps.Tests;

/// <summary>
/// A throwaway SQLite database with operations loaded from XML, so the whole pipeline runs for real.
/// </summary>
internal sealed class SqliteHarness : IDisposable
{
    private readonly string path;

    private SqliteHarness(string path, IDataOperations operations, AdoRecorder recorder)
    {
        this.path = path;
        this.Operations = operations;
        this.Recorder = recorder;
    }

    public IDataOperations Operations { get; }

    /// <summary>
    /// What the library handed to ADO.NET on this harness's connections.
    /// </summary>
    public AdoRecorder Recorder { get; }

    public string ConnectionString => $"Data Source={this.path}";

    public static SqliteHarness Create(string operationsXml, bool validate = true)
    {
        return Create([operationsXml], validate);
    }

    public static SqliteHarness Create(IEnumerable<string> operationsXml, bool validate = true)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dataops-{Guid.NewGuid():N}.db");
        var recorder = new AdoRecorder();

        var connections = new ConnectionSupplierRegistry("Main");
        connections.Register("Main", () => ConnectionReference.Owning(
            new RecordingConnection(new SqliteConnection($"Data Source={path}"), recorder),
            SqlProvider.Sqlite));

        var sources = operationsXml.Select(xml => (IOperationsDefinitionSource)XmlSource(xml, validate)).ToList();
        var registry = new CompositeOperationRegistry(new OperationRegistryInitializer(sources));

        var harness = new SqliteHarness(path, new DataOperations(registry, connections), recorder);
        harness.Execute("CREATE TABLE people (id INTEGER PRIMARY KEY, first_name TEXT NOT NULL, age INTEGER NOT NULL)");

        return harness;
    }

    public static XmlOperationsDefinitionSource XmlSource(string xml, bool validate = true)
    {
        return new XmlOperationsDefinitionSource([() => new MemoryStream(Encoding.UTF8.GetBytes(xml))], validate);
    }

    /// <summary>
    /// Runs SQL outside the library, for setting up and for checking what the library actually wrote.
    /// </summary>
    public void Execute(string sql)
    {
        using var connection = new SqliteConnection($"Data Source={this.path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public long CountPeople()
    {
        using var connection = new SqliteConnection($"Data Source={this.path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM people";
        return (long)command.ExecuteScalar()!;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(this.path);
    }
}
