namespace ApricotFramework.DataOps;

/// <summary>
/// The shape of result an operation returns, which decides the executor it may be called through.
/// </summary>
/// <remarks>
/// <see cref="Unknown"/> is permissive: an operation that declares nothing may be called through
/// any executor, and the caller carries the risk of a mismatch.
/// </remarks>
public enum ResultType
{
    /// <summary>
    /// The definition did not say.
    /// </summary>
    Unknown,

    /// <summary>
    /// A single result set.
    /// </summary>
    Table,

    /// <summary>
    /// Several result sets read in turn.
    /// </summary>
    MultipleTables,

    /// <summary>
    /// A single value from the first column of the first row.
    /// </summary>
    Scalar,

    /// <summary>
    /// The number of rows affected.
    /// </summary>
    RowCount
}
