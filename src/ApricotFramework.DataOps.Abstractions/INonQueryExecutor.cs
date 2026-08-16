namespace ApricotFramework.DataOps;

/// <summary>
/// Runs an operation that reports how many rows it affected.
/// </summary>
public interface INonQueryExecutor : IOperationExecutor<INonQueryExecutor>
{
    /// <summary>
    /// Runs the operation.
    /// </summary>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(object? param = null);
}
