namespace ApricotFramework.DataOps;

/// <summary>
/// The database engine an operation is written for.
/// </summary>
/// <remarks>
/// Member names are the values written into configuration and into the Compatibility attribute of
/// an operations file. Both are read case-insensitively, so earlier spellings still parse.
/// </remarks>
public enum SqlProvider
{
    /// <summary>
    /// Microsoft SQL Server.
    /// </summary>
    SqlServer,

    /// <summary>
    /// MySQL, and wire-compatible engines such as MariaDB.
    /// </summary>
    MySql,

    /// <summary>
    /// PostgreSQL.
    /// </summary>
    PostgreSql,

    /// <summary>
    /// SQLite.
    /// </summary>
    Sqlite
}
