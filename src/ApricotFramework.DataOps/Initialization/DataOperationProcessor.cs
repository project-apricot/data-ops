using ApricotFramework.DataOps.Definitions;

namespace ApricotFramework.DataOps.Initialization;

/// <summary>
/// Resolves declared operations into runnable ones, applying inherited settings and expanding each
/// operation across the providers it is compatible with.
/// </summary>
public static class DataOperationProcessor
{
    private static readonly SqlProvider[] AllProviders = Enum.GetValues<SqlProvider>();

    /// <summary>
    /// Processes one definition.
    /// </summary>
    /// <param name="definition">The definition to process.</param>
    /// <returns>The operations, keyed by operation and then by provider.</returns>
    public static Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>> Process(DataOperationsDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var result = new Dictionary<OpKey, Dictionary<SqlProvider, SqlOperation>>();

        var config = definition.Configuration ?? new DataConfiguration();

        foreach (var group in definition.Groups)
        {
            foreach (var op in group.Operations)
            {
                var opKey = OpKey.Of(group.Name, op.Name);

                // An operation overrides its group, which overrides the file-wide configuration.
                var autoTransaction = op.AutoTransaction ?? group.AutoTransaction ?? config.AutoTransaction;
                var timeout = op.Timeout ?? group.Timeout ?? config.Timeout;
                var compatibility = op.Providers ?? group.Providers ?? config.Providers ?? CompatibilityProviders.Any;

                var operation = new SqlOperation
                {
                    Name = opKey.Name,
                    Group = opKey.Group,
                    AutoTransaction = autoTransaction == AutoTransaction.No ? null : autoTransaction,
                    Timeout = timeout,
                    ResultType = op.Command?.ExpectedResult ?? ResultType.Unknown,
                    Type = op.Command?.Type ?? OperationType.Unknown,
                    Source = op.Command?.Source ?? string.Empty
                };

                foreach (var provider in AllProviders.Where(provider => compatibility.HasFlag(MapProvider(provider))))
                {
                    if (!result.TryGetValue(opKey, out var providerOps))
                    {
                        providerOps = [];
                        result.Add(opKey, providerOps);
                    }

                    providerOps[provider] = operation;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Maps a provider onto the flag that selects it in a Compatibility attribute.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>The matching flag.</returns>
    private static CompatibilityProviders MapProvider(SqlProvider provider)
    {
        return provider switch
        {
            SqlProvider.SqlServer => CompatibilityProviders.SqlServer,
            SqlProvider.MySql => CompatibilityProviders.MySql,
            SqlProvider.PostgreSql => CompatibilityProviders.PostgreSql,
            SqlProvider.Sqlite => CompatibilityProviders.Sqlite,
            _ => CompatibilityProviders.None
        };
    }
}
