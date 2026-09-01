using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.OrderStatuses;

// DTO payload for creating a new OrderStatus
public class OrderStatusCreateDto
{
    [Required(ErrorMessage = "اسم حالة الطلب مطلوب")]
    [StringLength(50, ErrorMessage = "اسم حالة الطلب يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}