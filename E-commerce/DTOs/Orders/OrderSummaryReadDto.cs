namespace E_commerce.DTOs.Orders;

// Summary projection model used for order listing endpoints
public class OrderSummaryReadDto
{
    public int OrderId { get; set; }

    public DateOnly OrderDate { get; set; }

    public string OrderStatusName { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public int ItemsCount { get; set; }
}