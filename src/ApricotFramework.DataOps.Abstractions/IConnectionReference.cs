using System.Data;

namespace ApricotFramework.DataOps;

/// <summary>
/// A connection together with the provider whose dialect its operations must be written in.
/// </summary>
/// <remarks>
/// Disposing a reference obtained from a data source closes the connection it created. A reference
/// wrapping a connection the caller supplied does not own it, and disposing the reference leaves
/// that connection open.
/// </remarks>
public interface IConnectionReference : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the connection.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// Gets the provider the connection speaks to.
    /// </summary>
    SqlProvider Provider { get; }
}
