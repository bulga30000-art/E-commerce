namespace E_commerce.DTOs.Orders;

public class OrderQueryParams
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

    // Optional status ID filter (e.g. filter by Shipped or Pending)
    public byte? OrderStatusId { get; set; }

    // Default sort direction for orders is newest first (descending)
    public bool SortDescending { get; set; } = true;
}