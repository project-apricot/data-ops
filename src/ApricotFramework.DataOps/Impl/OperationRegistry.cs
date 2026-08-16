namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// An operation registry filled in by hand, one operation at a time.
/// </summary>
public class OperationRegistry : IOperationRegistry
{
    private readonly Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>> operations = [];

    /// <summary>
    /// Registers an operation for a key and provider, replacing any already there.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <param name="provider">The provider the operation is written for.</param>
    /// <param name="operation">The operation.</param>
    public void Register(OpKey key, SqlProvider provider, SqlOperation operation)
    {
        if (!this.operations.TryGetValue(key, out var providerOps))
        {
            providerOps = [];
            this.operations.Add(key, providerOps);
        }

        providerOps[provider] = operation;
    }

    /// <inheritdoc />
    public bool IsDefined(OpKey key)
    {
        return this.operations.TryGetValue(key, out var ops) && ops.Count != 0;
    }

    /// <inheritdoc />
    public SqlOperation? GetOrNull(OpKey key, SqlProvider provider)
    {
        return this.operations.TryGetValue(key, out var providerOps)
            ? providerOps.GetValueOrDefault(provider)
            : null;
    }
}
