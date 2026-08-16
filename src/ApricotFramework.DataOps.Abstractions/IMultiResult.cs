namespace ApricotFramework.DataOps;

/// <summary>
/// Reads the result sets of a multi-result operation, in the order the command produced them.
/// </summary>
public interface IMultiResult : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Reads the next result set.
    /// </summary>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <returns>The rows.</returns>
    Task<IEnumerable<TResult>> ReadAsync<TResult>();

    /// <summary>
    /// Reads the next result set, mapping each row from two joined entities.
    /// </summary>
    /// <typeparam name="TFirst">The first entity type.</typeparam>
    /// <typeparam name="TSecond">The second entity type.</typeparam>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <param name="map">Combines the two entities into a row.</param>
    /// <param name="splitOn">The column at which the second entity starts.</param>
    /// <returns>The rows.</returns>
    Task<IEnumerable<TResult>> ReadAsync<TFirst, TSecond, TResult>(Func<TFirst, TSecond, TResult> map, string splitOn = "id");

    /// <summary>
    /// Reads the first row of the next result set.
    /// </summary>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <returns>The row, or the default when the set was empty.</returns>
    Task<TResult?> ReadFirstOrDefaultAsync<TResult>();
}
