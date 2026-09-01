using System;
using System.Collections.Generic;

namespace E_commerce.Models;

public partial class Shipper
{
    public int ShipperId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
