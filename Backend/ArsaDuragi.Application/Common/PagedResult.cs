namespace ArsaDuragi.Application.Common;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
    {
        Items = Items.Select(selector).ToList(),
        Page = Page,
        PageSize = PageSize,
        TotalCount = TotalCount
    };
}
