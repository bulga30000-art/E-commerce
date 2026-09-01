using System;
using System.Collections.Generic;

namespace E_commerce.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int QuantityInStock { get; set; }

    public decimal UnitPrice { get; set; }

    public byte CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
