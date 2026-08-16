using Dapper;

namespace ApricotFramework.DataOps;

/// <summary>
/// Controls how Dapper matches result columns to property names.
/// </summary>
/// <remarks>
/// <para>This is Dapper's own process-wide setting, so it applies to every query in the host, not
/// only to those run through this library.</para>
/// </remarks>
public static class DapperTypeMapping
{
    /// <summary>
    /// Chooses whether a column such as <c>user_id</c> maps to a property named <c>UserId</c>.
    /// </summary>
    /// <param name="enabled">True to ignore underscores when matching.</param>
    public static void SetMatchNamesWithUnderscores(bool enabled)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = enabled;
    }
}
