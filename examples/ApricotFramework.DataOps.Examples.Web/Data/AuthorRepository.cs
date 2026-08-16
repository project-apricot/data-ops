namespace ApricotFramework.DataOps.Examples.Web.Data;

/// <summary>
/// An author, whose columns differ from its property names by an underscore.
/// </summary>
/// <remarks>
/// Settable properties rather than a positional record: Dapper applies underscore matching when it
/// sets properties, but not when it matches constructor parameters.
/// </remarks>
public class Author
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the author's name, read from <c>full_name</c>.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year of birth, read from <c>born_in</c>.
    /// </summary>
    public int BornIn { get; set; }
}

/// <summary>
/// Reads and writes authors through data operations.
/// </summary>
public interface IAuthorRepository
{
    /// <summary>
    /// Lists the authors.
    /// </summary>
    /// <param name="byName">True to order by name rather than by identifier.</param>
    /// <returns>The authors.</returns>
    Task<IEnumerable<Author>> GetAll(bool byName);

    /// <summary>
    /// Lists the authors and their count, in one round trip.
    /// </summary>
    /// <returns>The authors and how many there are.</returns>
    Task<(IReadOnlyList<Author> Authors, long Count)> GetReport();

    /// <summary>
    /// Adds several authors in one transaction, rolling back if any of them fails.
    /// </summary>
    /// <param name="authors">The authors to add.</param>
    /// <returns>The number of authors after the transaction.</returns>
    Task<long> AddAll(IEnumerable<Author> authors);
}

/// <summary>
/// The data operations implementation of the author repository.
/// </summary>
public class AuthorRepository : IAuthorRepository
{
    private readonly IDataOperations dataOps;

    /// <summary>
    /// Creates a repository over data operations.
    /// </summary>
    /// <param name="dataOps">The data operations entry point.</param>
    public AuthorRepository(IDataOperations dataOps)
    {
        this.dataOps = dataOps;
    }

    /// <inheritdoc />
    public Task<IEnumerable<Author>> GetAll(bool byName)
    {
        return this.dataOps.Connect()
            .Query("Authors", "All")
            .WithBinding("byName", byName)
            .ExecuteAsync<Author>();
    }

    /// <inheritdoc />
    public Task<(IReadOnlyList<Author> Authors, long Count)> GetReport()
    {
        return this.dataOps.Connect()
            .MultiQuery("Reports", "AuthorsAndCount")
            .ExecuteAsync<(IReadOnlyList<Author>, long)>(async reader =>
            {
                var authors = await reader.ReadAsync<Author>();
                var count = await reader.ReadFirstOrDefaultAsync<long>();
                return (authors.ToList(), count);
            });
    }

    /// <inheritdoc />
    public async Task<long> AddAll(IEnumerable<Author> authors)
    {
        await using var scope = await this.dataOps.BeginAsync(AutoTransaction.Serializable);

        foreach (var author in authors)
        {
            await scope.Router.NonQuery("Authors", "Create").ExecuteAsync(new { author.FullName, author.BornIn });
        }

        var count = await scope.Router.Scalar("Authors", "Count").ExecuteAsync<long>();

        await scope.CommitAsync();

        return count;
    }
}
