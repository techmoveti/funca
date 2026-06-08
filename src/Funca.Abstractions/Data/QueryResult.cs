namespace Funca.Abstractions.Data;

/// <summary>
///     Query Result Abstraction - Represents the result of a paginated query.
/// </summary>
/// <param name="Data"></param>
/// <param name="Page"></param>
/// <param name="PageSize"></param>
/// <param name="RecordCount"></param>
/// <typeparam name="T"></typeparam>
public record QueryResult<T>(
    T? Data,
    int Page,
    int PageSize,
    long RecordCount)
{
    public int PageCount { get; init; } = (int)Math.Ceiling(RecordCount / (double)PageSize);
}