 // src/Library.Application/Authors/Common/PagedResult.cs
namespace Library.Application.Shareds;

public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page {  get; }
    public int PageSize { get; }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = new List<T>(items).AsReadOnly(); // copie defensive
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
