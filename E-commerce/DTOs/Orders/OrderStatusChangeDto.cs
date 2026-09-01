using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Orders;

// DTO payload for updating an existing order's status
public class OrderStatusChangeDto
{
    [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
    public byte NewStatusId { get; set; }
}