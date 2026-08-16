namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// The provider settings a definition level may carry.
/// </summary>
public interface IProviderOptions
{
    /// <summary>
    /// Gets or sets the providers to register for, or null to inherit from the enclosing level.
    /// </summary>
    CompatibilityProviders? Providers { get; set; }

    /// <summary>
    /// Gets or sets the command timeout, or null to inherit from the enclosing level.
    /// </summary>
    TimeSpan? Timeout { get; set; }
}
