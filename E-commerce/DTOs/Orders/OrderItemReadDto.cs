namespace E_commerce.DTOs.Orders;

public class OrderItemReadDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    // Historical unit price snapshot captured at order creation time
    public decimal UnitPrice { get; set; }

    // Computed total for this item (Quantity * UnitPrice)
    public decimal LineTotal => Quantity * UnitPrice;
}