using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Shippers;

// DTO payload for creating a new Shipper
public class ShipperCreateDto
{
    [Required(ErrorMessage = "اسم شركة الشحن مطلوب")]
    [StringLength(50, ErrorMessage = "اسم شركة الشحن يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}