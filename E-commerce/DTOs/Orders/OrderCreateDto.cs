using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Orders;

// DTO payload for creating a new order during checkout
public class OrderCreateDto
{
    // Optional shipper ID selected by customer
    public int? ShipperId { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
    public string? Comments { get; set; }

    [Required(ErrorMessage = "يجب أن يحتوي الطلب على عنصر واحد على الأقل")]
    [MinLength(1, ErrorMessage = "يجب أن يحتوي الطلب على عنصر واحد على الأقل")]
    public List<OrderItemCreateDto> Items { get; set; } = new();

    // Optional loyalty points redemption amount
    [Range(0, int.MaxValue, ErrorMessage = "عدد النقاط المطلوب صرفها غير صالح")]
    public int PointsToRedeem { get; set; } = 0;
}