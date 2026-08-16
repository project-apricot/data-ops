namespace ApricotFramework.DataOps;

/// <summary>
/// Runs an operation that returns several result sets.
/// </summary>
public interface IMultiQueryExecutor : IOperationExecutor<IMultiQueryExecutor>
{
    /// <summary>
    /// Runs the operation and reads its result sets.
    /// </summary>
    /// <typeparam name="TResult">The type the handler produces.</typeparam>
    /// <param name="resultHandler">Reads the sets in order; the reader is closed once it returns.</param>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <returns>Whatever the handler produced.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<IMultiResult, Task<TResult>> resultHandler, object? param = null);
}
