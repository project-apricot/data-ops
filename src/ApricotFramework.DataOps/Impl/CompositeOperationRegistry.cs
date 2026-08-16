using ApricotFramework.DataOps.Initialization;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// An operation registry built once from every registered definition source.
/// </summary>
public class CompositeOperationRegistry : IOperationRegistry
{
    private readonly Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>> operations;

    /// <summary>
    /// Reads every source and resolves its definitions into operations.
    /// </summary>
    /// <param name="initializer">The initializer over the registered sources.</param>
    public CompositeOperationRegistry(OperationRegistryInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        this.operations = initializer.Initialize();
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
