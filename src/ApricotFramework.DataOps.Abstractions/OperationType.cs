namespace ApricotFramework.DataOps;

/// <summary>
/// How the operation's command is expressed.
/// </summary>
public enum OperationType
{
    /// <summary>
    /// The definition did not say; the command is treated as text.
    /// </summary>
    Unknown,

    /// <summary>
    /// The command carries its own SQL.
    /// </summary>
    Text,

    /// <summary>
    /// The command names a stored procedure.
    /// </summary>
    StoredProcedure
}
