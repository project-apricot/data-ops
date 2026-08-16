namespace ApricotFramework.DataOps;

/// <summary>
/// Runs an operation that returns a single value.
/// </summary>
public interface IScalarExecutor : IOperationExecutor<IScalarExecutor>
{
    /// <summary>
    /// Runs the operation.
    /// </summary>
    /// <typeparam name="TResult">The value type.</typeparam>
    /// <param name="param">The parameters, or null when the operation takes none.</param>
    /// <returns>The value, or the default when the operation returned no rows.</returns>
    Task<TResult?> ExecuteAsync<TResult>(object? param = null);
}
