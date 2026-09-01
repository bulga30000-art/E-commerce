using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Payments;

// DTO payload for updating a pending cash payment status
public class PaymentStatusUpdateDto
{
    // Allowed values: "Completed" or "Failed" (see Common/PaymentStatuses.cs)
    [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
    public string NewStatus { get; set; } = null!;
}