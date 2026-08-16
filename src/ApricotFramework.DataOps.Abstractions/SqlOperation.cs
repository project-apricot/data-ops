namespace ApricotFramework.DataOps;

/// <summary>
/// An operation resolved for one provider, with its inherited settings already applied.
/// </summary>
public class SqlOperation
{
    /// <summary>
    /// Gets or sets the name of the operation within its group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the group the operation is declared in.
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how the command is expressed.
    /// </summary>
    public OperationType Type { get; set; }

    /// <summary>
    /// Gets or sets the SQL text, or the stored procedure name for <see cref="OperationType.StoredProcedure"/>.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shape of result the operation declares.
    /// </summary>
    public ResultType ResultType { get; set; }

    /// <summary>
    /// Gets or sets the transaction to open around the operation, or null to run without one.
    /// </summary>
    public AutoTransaction? AutoTransaction { get; set; }

    /// <summary>
    /// Gets or sets the command timeout, or null to leave it to the provider.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
