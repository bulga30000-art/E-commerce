namespace E_commerce.DTOs.Payments;

public class PaymentReadDto
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public DateOnly PaymentDate { get; set; }

    // Snapshot of order total amount captured at payment creation
    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;
}