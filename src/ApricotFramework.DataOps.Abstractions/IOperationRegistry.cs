namespace ApricotFramework.DataOps;

/// <summary>
/// Holds the operations loaded from every definition source, resolved per provider.
/// </summary>
public interface IOperationRegistry
{
    /// <summary>
    /// Checks whether the key is defined for at least one provider.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>True when the key is known.</returns>
    bool IsDefined(OpKey key);

    /// <summary>
    /// Gets the operation for a key and provider.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <param name="provider">The provider to resolve for.</param>
    /// <returns>The operation, or null when it is not defined for that provider.</returns>
    SqlOperation? GetOrNull(OpKey key, SqlProvider provider);
}
