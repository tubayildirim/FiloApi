namespace Filo.Common.Models;

public class PaginationParams
{
    private const int MaxPageSize = 50;
    
    private int _pageNumber = 1;
    public int? PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value ?? 1;
    }

    private int _pageSize = 10;
    public int? PageSize
    {
        get => _pageSize;
        set => _pageSize = value.HasValue ? (value.Value > MaxPageSize ? MaxPageSize : (value.Value < 1 ? 1 : value.Value)) : 10;
    }

    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } // "asc" or "desc"
}
