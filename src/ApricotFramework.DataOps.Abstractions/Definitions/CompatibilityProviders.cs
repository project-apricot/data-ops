namespace ApricotFramework.DataOps.Definitions;

/// <summary>
/// The providers an operation is written for, as a set.
/// </summary>
/// <remarks>
/// Written in an operations file as a whitespace-separated list, so <c>Compatibility="MySql
/// PostgreSql"</c> registers the operation for both and for neither of the others.
/// </remarks>
[Flags]
public enum CompatibilityProviders : long
{
    /// <summary>
    /// No provider, which registers the operation for none of them.
    /// </summary>
    None = 0,

    /// <summary>
    /// Microsoft SQL Server.
    /// </summary>
    SqlServer = 1,

    /// <summary>
    /// MySQL, and wire-compatible engines such as MariaDB.
    /// </summary>
    MySql = 2,

    /// <summary>
    /// PostgreSQL.
    /// </summary>
    PostgreSql = 4,

    /// <summary>
    /// SQLite.
    /// </summary>
    Sqlite = 8,

    /// <summary>
    /// Every provider the library knows about.
    /// </summary>
    Any = SqlServer | MySql | PostgreSql | Sqlite
}
