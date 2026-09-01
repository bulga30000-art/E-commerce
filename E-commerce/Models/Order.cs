using System;
using System.Collections.Generic;

namespace E_commerce.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly OrderDate { get; set; }

    public byte? OrderStatusId { get; set; }

    public string? Comments { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public int? ShipperId { get; set; }

    public decimal? TotalAmount { get; set; }

    // Points redeemed by the customer during checkout for discount deduction.
    // Stored persistently to track exact points to restore if the order is cancelled.
    public int PointsRedeemed { get; set; }

    // Points earned when order status transitions to Delivered.
    // Remains 0 until delivery confirmation to prevent premature point distribution.
    public int PointsEarned { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus? OrderStatus { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Shipper? Shipper { get; set; }
}