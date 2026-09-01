namespace E_commerce.DTOs.Orders;

public class OrderReadDto
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly OrderDate { get; set; }

    public byte OrderStatusId { get; set; }

    public string OrderStatusName { get; set; } = null!;

    public string? Comments { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public int? ShipperId { get; set; }

    public string? ShipperName { get; set; }

    // Net payable total amount after applying discounts and loyalty points
    public decimal TotalAmount { get; set; }

    // Loyalty points redeemed during order creation
    public int PointsRedeemed { get; set; }

    // Loyalty points earned upon order delivery
    public int PointsEarned { get; set; }

    public List<OrderItemReadDto> Items { get; set; } = new();
}