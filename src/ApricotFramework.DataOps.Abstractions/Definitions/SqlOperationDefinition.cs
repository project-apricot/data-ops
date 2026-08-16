namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// A single operation as declared, before its inherited settings are applied.
/// </summary>
public class SqlOperationDefinition : ITransactionOptions, IProviderOptions
{
    /// <summary>
    /// Gets or sets the name of the operation within its group.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the command to run.
    /// </summary>
    public CommandDefinition? Command { get; set; }

    /// <summary>
    /// Gets or sets the group this operation was declared in.
    /// </summary>
    public OperationGroup? Group { get; set; }

    /// <inheritdoc />
    public AutoTransaction? AutoTransaction { get; set; }

    /// <inheritdoc />
    public CompatibilityProviders? Providers { get; set; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; set; }
}
