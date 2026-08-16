namespace ApricotFramework.DataOps.AspNetCore.Options;

/// <summary>
/// The data sources available to the host, and how results map onto properties.
/// </summary>
public class DataOperationsOptions
{
    /// <summary>
    /// The configuration section these options bind from.
    /// </summary>
    public const string SectionName = "DataOperations";

    /// <summary>
    /// Gets or sets the data source used when a caller does not name one.
    /// </summary>
    public string? DefaultDataSource { get; set; }

    /// <summary>
    /// Gets the data sources, keyed by the name callers address them with.
    /// </summary>
    public IDictionary<string, DataSourceOptions> DataSources { get; } = new Dictionary<string, DataSourceOptions>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets whether a column such as <c>user_id</c> maps to a property named <c>UserId</c>.
    /// </summary>
    /// <remarks>
    /// This is Dapper's own process-wide setting, so turning it off also changes queries the host
    /// runs through Dapper directly.
    /// </remarks>
    public bool MatchNamesWithUnderscores { get; set; } = true;
}
