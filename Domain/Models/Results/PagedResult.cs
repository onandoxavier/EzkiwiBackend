namespace VirtualQueueApi.Domain.Models.Results;

public class PagedResult<T>
{
    public IList<T> Items { get; set; } = [];
    public int Total { get; set; } = 0;
    public int Page { get; set; } = 0;
    public int Rows { get; set; } = 0;
    public int TotalPages { get; set; } = 0;

    public PagedResult() { }

    public PagedResult(IList<T> items, int total, int page, int rows)
    {
        Items = items;
        Total = total;
        Page = page;
        Rows = rows;
        TotalPages = (int)Math.Ceiling(total / (double)rows);
    }
}
