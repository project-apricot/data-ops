namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// The command an operation runs, as declared.
/// </summary>
public class CommandDefinition
{
    /// <summary>
    /// Gets or sets the SQL text, or the stored procedure name.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets how the command is expressed.
    /// </summary>
    public OperationType Type { get; set; }

    /// <summary>
    /// Gets or sets the shape of result the command declares.
    /// </summary>
    public ResultType ExpectedResult { get; set; }
}
