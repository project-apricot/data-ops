namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// The settings every group and operation in one definition file inherits from.
/// </summary>
public class DataConfiguration : ITransactionOptions, IProviderOptions
{
    /// <inheritdoc />
    public AutoTransaction? AutoTransaction { get; set; }

    /// <inheritdoc />
    public CompatibilityProviders? Providers { get; set; }

    /// <inheritdoc />
    public TimeSpan? Timeout { get; set; }
}
