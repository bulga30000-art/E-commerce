using System;
using System.Collections.Generic;

namespace E_commerce.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    //public DateOnly? BirthDate { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public int Points { get; set; }

    public string? UserId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
