using ApricotFramework.DataOps.Definitions;

namespace ApricotFramework.DataOps;

/// <summary>
/// Contributes operation definitions to the registry.
/// </summary>
/// <remarks>
/// Sources are additive: a library registers its own and the host registers its own, and every one
/// of them is read when the registry is first built.
/// </remarks>
public interface IOperationsDefinitionSource
{
    /// <summary>
    /// Reads every definition this source holds.
    /// </summary>
    /// <returns>The definitions.</returns>
    IEnumerable<DataOperationsDefinition> GetAll();
}
