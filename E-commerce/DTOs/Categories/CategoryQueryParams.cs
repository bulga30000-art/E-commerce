namespace E_commerce.DTOs.Categories;

// Query parameter model for pagination, searching, and sorting categories
public class CategoryQueryParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : Math.Min(value, MaxPageSize);
    }

    public string? SearchTerm { get; set; }

    public bool SortDescending { get; set; } = false;
}