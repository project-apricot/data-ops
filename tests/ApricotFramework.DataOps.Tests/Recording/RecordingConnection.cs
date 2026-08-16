using System.Data;
using System.Data.Common;

namespace ApricotFramework.DataOps.Tests.Recording;

/// <summary>
/// A SQLite connection that records the command type, timeout and isolation level the library asks
/// for, so those can be asserted without a second database engine.
/// </summary>
internal sealed class RecordingConnection : DbConnection
{
    private readonly AdoRecorder recorder;

    public RecordingConnection(DbConnection inner, AdoRecorder recorder)
    {
        this.Inner = inner;
        this.recorder = recorder;
    }

    public DbConnection Inner { get; }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string ConnectionString
    {
        get => this.Inner.ConnectionString;
        set => this.Inner.ConnectionString = value;
    }

    public override string Database => this.Inner.Database;

    public override string DataSource => this.Inner.DataSource;

    public override string ServerVersion => this.Inner.ServerVersion;

    public override ConnectionState State => this.Inner.State;

    public override void ChangeDatabase(string databaseName) => this.Inner.ChangeDatabase(databaseName);

    public override void Open() => this.Inner.Open();

    public override Task OpenAsync(CancellationToken cancellationToken) => this.Inner.OpenAsync(cancellationToken);

    public override void Close() => this.Inner.Close();

    protected override DbCommand CreateDbCommand() => new RecordingCommand(this.Inner.CreateCommand(), this.recorder);

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        this.recorder.Transactions.Add(isolationLevel);

        // The inner transaction is returned directly, so a command enlists in it unwrapped.
        return this.Inner.BeginTransaction(isolationLevel);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Inner.Dispose();
        }

        base.Dispose(disposing);
    }
}
