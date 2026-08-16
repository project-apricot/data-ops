using System.Data;

namespace ApricotFramework.DataOps;

/// <summary>
/// The settings every executor accepts before it runs, each returning the executor for chaining.
/// </summary>
/// <typeparam name="TExecutor">The concrete executor being configured.</typeparam>
public interface IOperationExecutor<out TExecutor>
{
    /// <summary>
    /// Gets the connection the operation runs on.
    /// </summary>
    IConnectionReference ConnectionReference { get; }

    /// <summary>
    /// Gets the transaction the operation runs in if one was supplied or opened.
    /// </summary>
    IDbTransaction? Transaction { get; }

    /// <summary>
    /// Gets whether the executor commits the transaction itself.
    /// </summary>
    bool AutoCommit { get; }

    /// <summary>
    /// Gets whether results are read into memory before being returned.
    /// </summary>
    bool Buffered { get; }

    /// <summary>
    /// Gets the transaction opened around this operation, or null to run without one.
    /// </summary>
    AutoTransaction? AutoTransaction { get; }

    /// <summary>
    /// Gets the command timeout, or null to leave it to the provider.
    /// </summary>
    TimeSpan? Timeout { get; }

    /// <summary>
    /// Gets the bindings that resolve the conditional fragments of the command.
    /// </summary>
    IReadOnlyDictionary<string, object?> Bindings { get; }

    /// <summary>
    /// Runs the operation in a transaction the caller owns.
    /// </summary>
    /// <param name="transaction">The transaction to enlist in.</param>
    /// <param name="autocommit">Whether to commit it once the operation succeeds.</param>
    /// <returns>The executor.</returns>
    TExecutor WithTransaction(IDbTransaction? transaction, bool autocommit = false);

    /// <summary>
    /// Opens a transaction around the operation, replacing whatever its definition declared.
    /// </summary>
    /// <param name="transaction">The isolation to open at, or null to run without one.</param>
    /// <returns>The executor.</returns>
    TExecutor WithTransaction(AutoTransaction? transaction);

    /// <summary>
    /// Overrides the command timeout.
    /// </summary>
    /// <param name="timeout">The timeout, or null to leave it to the provider.</param>
    /// <returns>The executor.</returns>
    TExecutor WithTimeout(TimeSpan? timeout);

    /// <summary>
    /// Chooses whether results are read into memory before being returned.
    /// </summary>
    /// <param name="buffered">True to buffer.</param>
    /// <returns>The executor.</returns>
    TExecutor WithBuffering(bool buffered = true);

    /// <summary>
    /// Supplies a binding used to resolve a conditional fragment of the command.
    /// </summary>
    /// <param name="binding">The binding name.</param>
    /// <param name="value">The value; a false or null value, takes the else branch.</param>
    /// <returns>The executor.</returns>
    TExecutor WithBinding(string binding, object? value);
}
