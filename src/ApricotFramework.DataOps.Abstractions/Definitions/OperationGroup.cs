namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// A named set of operations sharing settings.
/// </summary>
public class OperationGroup : ITransactionOptions, IProviderOptions
{
    /// <summary>
    /// Gets or sets the group name, which is the first half of every key in it.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the operations declared in this group.
    /// </summary>
    public IList<SqlOperationDefinition> Operations { get; } = [];

    /// <inheritdoc />
    public AutoTransaction? AutoTransaction { get; set; }

    /// <inheritdoc />
    public CompatibilityProviders? Providers { get; set; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; set; }
}
