namespace ApricotFramework.DataOps;

/// <summary>
/// Addresses operations on a router by group and name instead of building an <see cref="OpKey"/>.
/// </summary>
public static class OperationRouterExtensions
{
    /// <summary>
    /// Resolves an operation returning a result set.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="group">The group name.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IQueryExecutor Query(this IOperationRouter router, string group, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.Query(OpKey.Of(group, operation));
    }

    /// <summary>
    /// Resolves an operation declared without a group.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IQueryExecutor Query(this IOperationRouter router, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.Query(OpKey.Of(operation));
    }

    /// <summary>
    /// Resolves an operation returning a single row.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="group">The group name.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IQueryFirstExecutor QueryFirst(this IOperationRouter router, string group, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.QueryFirst(OpKey.Of(group, operation));
    }

    /// <summary>
    /// Resolves an operation declared without a group.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IQueryFirstExecutor QueryFirst(this IOperationRouter router, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.QueryFirst(OpKey.Of(operation));
    }

    /// <summary>
    /// Resolves an operation reporting how many rows it affected.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="group">The group name.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static INonQueryExecutor NonQuery(this IOperationRouter router, string group, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.NonQuery(OpKey.Of(group, operation));
    }

    /// <summary>
    /// Resolves an operation declared without a group.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static INonQueryExecutor NonQuery(this IOperationRouter router, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.NonQuery(OpKey.Of(operation));
    }

    /// <summary>
    /// Resolves an operation returning several result sets.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="group">The group name.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IMultiQueryExecutor MultiQuery(this IOperationRouter router, string group, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.MultiQuery(OpKey.Of(group, operation));
    }

    /// <summary>
    /// Resolves an operation declared without a group.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IMultiQueryExecutor MultiQuery(this IOperationRouter router, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.MultiQuery(OpKey.Of(operation));
    }

    /// <summary>
    /// Resolves an operation returning a single value.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="group">The group name.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IScalarExecutor Scalar(this IOperationRouter router, string group, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.Scalar(OpKey.Of(group, operation));
    }

    /// <summary>
    /// Resolves an operation declared without a group.
    /// </summary>
    /// <param name="router">The router.</param>
    /// <param name="operation">The operation name.</param>
    /// <returns>The executor.</returns>
    public static IScalarExecutor Scalar(this IOperationRouter router, string operation)
    {
        ArgumentNullException.ThrowIfNull(router);
        return router.Scalar(OpKey.Of(operation));
    }
}
