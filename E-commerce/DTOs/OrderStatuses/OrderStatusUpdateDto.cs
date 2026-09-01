using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.OrderStatuses;

// DTO payload for updating an existing OrderStatus
public class OrderStatusUpdateDto
{
    [Required(ErrorMessage = "اسم حالة الطلب مطلوب")]
    [StringLength(50, ErrorMessage = "اسم حالة الطلب يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}