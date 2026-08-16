namespace ApricotFramework.DataOps;

/// <summary>
/// Runs an operation that returns a result set.
/// </summary>
public interface IQueryExecutor : IOperationExecutor<IQueryExecutor>
{
    /// <summary>
    /// Runs the operation and maps every row.
    /// </summary>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <returns>The rows.</returns>
    Task<IEnumerable<TResult>> ExecuteAsync<TResult>(object? param = null);

    /// <summary>
    /// Runs the operation and maps each row from two joined entities.
    /// </summary>
    /// <typeparam name="TFirst">The first entity type.</typeparam>
    /// <typeparam name="TSecond">The second entity type.</typeparam>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <param name="map">Combines the two entities into a row.</param>
    /// <param name="splitOn">The column at which the second entity starts.</param>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <returns>The rows.</returns>
    Task<IEnumerable<TResult>> ExecuteAsync<TFirst, TSecond, TResult>(Func<TFirst, TSecond, TResult> map, string splitOn = "id", object? param = null);
}
