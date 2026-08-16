using Dapper;

namespace ApricotFramework.DataOps.Impl;

/// <summary>
/// Reads the result sets of a multi-result operation from a Dapper grid reader.
/// </summary>
public sealed class DapperMultiResult : IMultiResult
{
    private readonly SqlMapper.GridReader gridReader;
    private readonly bool buffered;

    /// <summary>
    /// Wraps a grid reader.
    /// </summary>
    /// <param name="gridReader">The reader.</param>
    /// <param name="buffered">Whether each set is read into memory before being returned.</param>
    public DapperMultiResult(SqlMapper.GridReader gridReader, bool buffered)
    {
        this.gridReader = gridReader;
        this.buffered = buffered;
    }

    /// <inheritdoc />
    public Task<IEnumerable<TResult>> ReadAsync<TResult>()
    {
        return this.gridReader.ReadAsync<TResult>(this.buffered);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Dapper's grid reader has no asynchronous multi-mapping read, so this one completes
    /// synchronously and blocks the calling thread while the set is read.
    /// </remarks>
    public Task<IEnumerable<TResult>> ReadAsync<TFirst, TSecond, TResult>(Func<TFirst, TSecond, TResult> map, string splitOn = "id")
    {
        return Task.FromResult(this.gridReader.Read(map, splitOn, this.buffered));
    }

    /// <inheritdoc />
    public Task<TResult?> ReadFirstOrDefaultAsync<TResult>()
    {
        return this.gridReader.ReadFirstOrDefaultAsync<TResult>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.gridReader.Dispose();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return this.gridReader.DisposeAsync();
    }
}
