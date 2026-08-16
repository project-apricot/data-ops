namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// The transaction setting a definition level may carry.
/// </summary>
public interface ITransactionOptions
{
    /// <summary>
    /// Gets or sets the transaction to open, or null to inherit from the enclosing level.
    /// </summary>
    AutoTransaction? AutoTransaction { get; set; }
}
