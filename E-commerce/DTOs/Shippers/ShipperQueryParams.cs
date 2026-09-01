namespace E_commerce.DTOs.Shippers;

// Query parameter model for pagination, searching, and sorting shippers
public class ShipperQueryParams
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