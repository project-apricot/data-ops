using System.Data;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// An executor that runs one resolved operation.
/// </summary>
/// <typeparam name="TExecutor">The concrete executor being configured.</typeparam>
public abstract class DapperOperationExecutorBase<TExecutor> : DapperExecutorBase<TExecutor>
{
    /// <summary>
    /// Creates an executor for an operation.
    /// </summary>
    /// <param name="connectionReference">The connection to run on.</param>
    /// <param name="operation">The operation to run.</param>
    protected DapperOperationExecutorBase(IConnectionReference connectionReference, SqlOperation operation)
        : base(connectionReference, GetAutoTransaction(operation), operation.Timeout)
    {
        this.Operation = operation;
    }

    /// <summary>
    /// Gets the operation being run.
    /// </summary>
    public SqlOperation Operation { get; }

    /// <summary>
    /// Gets the timeout in whole seconds, as ADO.NET expects it.
    /// </summary>
    /// <returns>The timeout, or null to leave it to the provider.</returns>
    protected int? GetEffectiveTimeout()
    {
        return this.Timeout.HasValue ? (int)this.Timeout.Value.TotalSeconds : null;
    }

    /// <summary>
    /// Gets the command text with its conditional fragments resolved.
    /// </summary>
    /// <returns>The command text.</returns>
    protected string GetEffectiveSource()
    {
        return OperationSourceTemplate.Resolve(this.Operation.Source, this.Bindings);
    }

    /// <summary>
    /// Gets the command type ADO.NET should use.
    /// </summary>
    /// <returns>The command type.</returns>
    protected CommandType GetEffectiveCommandType()
    {
        return this.Operation.Type == OperationType.StoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
    }

    private static AutoTransaction? GetAutoTransaction(SqlOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return operation.AutoTransaction;
    }
}
