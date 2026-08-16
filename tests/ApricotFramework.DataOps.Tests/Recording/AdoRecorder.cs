using System.Data;

namespace ApricotFramework.DataOps.Tests.Recording;

/// <summary>
/// What the library handed to ADO.NET, captured at execution time.
/// </summary>
/// <param name="CommandText">The command text after conditional fragments were resolved.</param>
/// <param name="CommandType">The command type the library asked for, before any substitution.</param>
/// <param name="CommandTimeout">The timeout in whole seconds, as ADO.NET received it.</param>
internal sealed record RecordedCommand(string CommandText, CommandType CommandType, int CommandTimeout);

/// <summary>
/// Collects the calls a recording connection observes.
/// </summary>
/// <remarks>
/// This is the whole provider-specific surface the library owns: it selects an authored statement
/// and passes it to ADO.NET with a command type, a timeout and an isolation level. What a driver
/// does afterwards is not this library's behaviour, so it is not what these tests assert.
/// </remarks>
internal sealed class AdoRecorder
{
    public List<RecordedCommand> Commands { get; } = [];

    public List<IsolationLevel> Transactions { get; } = [];

    public RecordedCommand LastCommand => this.Commands.Count > 0
        ? this.Commands[^1]
        : throw new InvalidOperationException("No command was executed.");
}
