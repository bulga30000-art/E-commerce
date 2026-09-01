using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Orders;

// Single line item in order checkout request payload
public class OrderItemCreateDto
{
    [Required(ErrorMessage = "رقم المنتج مطلوب")]
    [Range(1, int.MaxValue, ErrorMessage = "رقم المنتج غير صالح")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون رقماً أكبر من صفر")]
    public int Quantity { get; set; }
}