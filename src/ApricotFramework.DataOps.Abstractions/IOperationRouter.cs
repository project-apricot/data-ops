namespace ApricotFramework.DataOps;

/// <summary>
/// Resolves an operation key into the executor its declared result allows.
/// </summary>
/// <remarks>
/// Every executor from one router shares that router's connection, so a router obtained inside a
/// transaction scope runs all its operations in that transaction. Disposing a router closes the
/// connection when the router owns it.
/// </remarks>
public interface IOperationRouter : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Resolves an operation returning a result set.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>The executor.</returns>
    IQueryExecutor Query(OpKey key);

    /// <summary>
    /// Resolves an operation returning a single row.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>The executor.</returns>
    IQueryFirstExecutor QueryFirst(OpKey key);

    /// <summary>
    /// Resolves an operation reporting how many rows it affected.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>The executor.</returns>
    INonQueryExecutor NonQuery(OpKey key);

    /// <summary>
    /// Resolves an operation returning several result sets.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>The executor.</returns>
    IMultiQueryExecutor MultiQuery(OpKey key);

    /// <summary>
    /// Resolves an operation returning a single value.
    /// </summary>
    /// <param name="key">The operation key.</param>
    /// <returns>The executor.</returns>
    IScalarExecutor Scalar(OpKey key);
}
