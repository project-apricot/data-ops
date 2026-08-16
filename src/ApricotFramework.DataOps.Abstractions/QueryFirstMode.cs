namespace ApricotFramework.DataOps;

/// <summary>
/// How a single-row query treats a row count other than one.
/// </summary>
public enum QueryFirstMode
{
    /// <summary>
    /// Return the first row, or the default value when there are none.
    /// </summary>
    FirstOrDefault,

    /// <summary>
    /// Return the first row, throwing when there are none.
    /// </summary>
    First,

    /// <summary>
    /// Return the only row, or the default value when there are none, throwing when there are more.
    /// </summary>
    SingleRowOrDefault,

    /// <summary>
    /// Return the only row, throwing when there is not exactly one.
    /// </summary>
    SingleRow
}
