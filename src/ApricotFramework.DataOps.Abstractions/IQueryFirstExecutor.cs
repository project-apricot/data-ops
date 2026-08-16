namespace ApricotFramework.DataOps;

/// <summary>
/// Runs an operation that returns a single row.
/// </summary>
public interface IQueryFirstExecutor : IOperationExecutor<IQueryFirstExecutor>
{
    /// <summary>
    /// Runs the operation and maps one row.
    /// </summary>
    /// <typeparam name="TResult">The row type.</typeparam>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <param name="mode">How a row count other than one is treated.</param>
    /// <returns>The row, or the default value where the mode allows it.</returns>
    Task<TResult?> ExecuteAsync<TResult>(object? param = null, QueryFirstMode mode = QueryFirstMode.FirstOrDefault);
}
