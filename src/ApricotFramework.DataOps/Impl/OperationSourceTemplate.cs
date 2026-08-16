using System.Text.RegularExpressions;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Resolves the conditional fragments of a command against the bindings supplied to an executor.
/// </summary>
/// <remarks>
/// A fragment selects between two pieces of SQL the operation's author wrote; a binding value is
/// never substituted into the command, so bindings cannot inject SQL the way a parameter could.
/// </remarks>
internal static partial class OperationSourceTemplate
{
    /// <summary>
    /// Replaces every <c>{if:name{ … }else{ … }}</c> fragment with the branch its binding selects.
    /// </summary>
    /// <param name="source">The command text.</param>
    /// <param name="bindings">The bindings supplied to the executor.</param>
    /// <returns>The resolved command text.</returns>
    public static string Resolve(string source, IReadOnlyDictionary<string, object?> bindings)
    {
        return ConditionalFragment().Replace(source, match =>
        {
            var binding = match.Groups["binding"].Value.Trim();

            if (string.IsNullOrWhiteSpace(binding) || !bindings.TryGetValue(binding, out var value))
            {
                value = null;
            }

            // A bool binding is its own condition; any other non-null value counts as present.
            var selectIf = value is bool flag ? flag : value is not null;

            return selectIf
                ? match.Groups["if"].Value.Trim()
                : match.Groups["else"].Value.Trim();
        });
    }

    [GeneratedRegex(@"{\s*if:(?<binding>[^{}]*)\s*{\s*(?<if>[^{}]*)\s*}\s*else?\s*{\s*(?<else>[^{}]*)\s*}\s*}")]
    private static partial Regex ConditionalFragment();
}
