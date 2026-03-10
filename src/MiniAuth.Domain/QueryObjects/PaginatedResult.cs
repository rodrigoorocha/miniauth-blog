namespace MiniAuth.Domain.QueryObjects;

public class PaginatedResult<T> where T : class
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
}
