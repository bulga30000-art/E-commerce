using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Payments;

// DTO payload for creating an order payment
public class PaymentCreateDto
{
    // Allowed values: "CreditCard" or "Cash" (see Common/PaymentMethods.cs)
    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    public string PaymentMethod { get; set; } = null!;
}