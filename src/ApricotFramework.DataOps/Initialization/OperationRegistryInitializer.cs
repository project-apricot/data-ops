namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Merges the operations of every registered definition source into one set.
/// </summary>
public class OperationRegistryInitializer
{
    private readonly IEnumerable<IOperationsDefinitionSource> sources;

    /// <summary>
    /// Creates an initializer over the registered sources.
    /// </summary>
    /// <param name="sources">The sources to read.</param>
    public OperationRegistryInitializer(IEnumerable<IOperationsDefinitionSource> sources)
    {
        this.sources = sources;
    }

    /// <summary>
    /// Reads every source and merges the result.
    /// </summary>
    /// <returns>The operations, keyed by operation and then by provider.</returns>
    /// <exception cref="IncorrectOperationException">
    /// Two sources define the same operation for the same provider, so which one would run is
    /// decided by registration order rather than by anything the definitions say.
    /// </exception>
    public Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>> Initialize()
    {
        var operations = new Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>>();

        foreach (var source in this.sources)
        {
            foreach (var operationSet in source.GetAll().Select(DataOperationProcessor.Process))
            {
                foreach (var (opKey, ops) in operationSet)
                {
                    if (!operations.TryGetValue(opKey, out var operationsByKey))
                    {
                        operationsByKey = [];
                        operations[opKey] = operationsByKey;
                    }

                    foreach (var (provider, operation) in ops)
                    {
                        if (!operationsByKey.TryAdd(provider, operation))
                        {
                            throw new IncorrectOperationException($"The operation '{opKey}' with provider '{provider}' is already defined.");
                        }
                    }
                }
            }
        }

        return operations;
    }
}
