using System.Data;
using System.Data.Common;

namespace ApricotFramework.DataOps.Tests.Recording;

/// <summary>
/// Records what the library set on a command, then runs something SQLite can actually execute.
/// </summary>
internal sealed class RecordingCommand : DbCommand
{
    /// <summary>
    /// Substituted for a stored procedure call, which SQLite has no concept of and whose
    /// <see cref="CommandType"/> its command object rejects outright.
    /// </summary>
    private const string StoredProcedureStandIn = "SELECT 1";

    private readonly DbCommand inner;
    private readonly AdoRecorder recorder;

    private CommandType requestedType = CommandType.Text;

    public RecordingCommand(DbCommand inner, AdoRecorder recorder)
    {
        this.inner = inner;
        this.recorder = recorder;
    }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string CommandText
    {
        get => this.inner.CommandText;
        set => this.inner.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => this.inner.CommandTimeout;
        set => this.inner.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => this.requestedType;
        set
        {
            this.requestedType = value;

            // Keep the inner command runnable; the assertion is about what was asked for.
            this.inner.CommandType = value == CommandType.StoredProcedure ? CommandType.Text : value;
        }
    }

    public override bool DesignTimeVisible
    {
        get => this.inner.DesignTimeVisible;
        set => this.inner.DesignTimeVisible = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => this.inner.UpdatedRowSource;
        set => this.inner.UpdatedRowSource = value;
    }

    protected override DbConnection? DbConnection
    {
        get => this.inner.Connection;
        set => this.inner.Connection = value is RecordingConnection recording ? recording.Inner : value;
    }

    protected override DbParameterCollection DbParameterCollection => this.inner.Parameters;

    protected override DbTransaction? DbTransaction
    {
        get => this.inner.Transaction;
        set => this.inner.Transaction = value;
    }

    public override void Cancel() => this.inner.Cancel();

    public override void Prepare() => this.inner.Prepare();

    public override int ExecuteNonQuery()
    {
        this.Record();
        return this.inner.ExecuteNonQuery();
    }

    public override object? ExecuteScalar()
    {
        this.Record();
        return this.inner.ExecuteScalar();
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        this.Record();
        return this.inner.ExecuteNonQueryAsync(cancellationToken);
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        this.Record();
        return this.inner.ExecuteScalarAsync(cancellationToken);
    }

    protected override DbParameter CreateDbParameter() => this.inner.CreateParameter();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        this.Record();
        return this.inner.ExecuteReader(behavior);
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        this.Record();
        return this.inner.ExecuteReaderAsync(behavior, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.inner.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Record()
    {
        this.recorder.Commands.Add(new RecordedCommand(this.inner.CommandText, this.requestedType, this.inner.CommandTimeout));

        if (this.requestedType == CommandType.StoredProcedure)
        {
            this.inner.CommandText = StoredProcedureStandIn;
        }
    }
}
