namespace E_commerce.DTOs.Products;

// Query parameter model bound via [FromQuery] for filtering, sorting, and pagination of products
public class ProductQueryParams
{
    // Maximum allowed page size to prevent database overload
    private const int MaxPageSize = 50;

    private int _pageSize = 10;
    private int _pageNumber = 1;

    // Enforce positive page number (default 1)
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    // Enforce page size boundaries between 1 and MaxPageSize
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : Math.Min(value, MaxPageSize);
    }

    // Optional search filter for Product.Name
    public string? SearchTerm { get; set; }

    // Optional sort field ("name" or "price")
    public string? SortBy { get; set; }

    // Sort order direction (true for descending, false for ascending)
    public bool SortDescending { get; set; } = false;
}