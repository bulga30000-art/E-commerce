using System;
using System.Collections.Generic;

namespace E_commerce.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public DateOnly PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }
}
