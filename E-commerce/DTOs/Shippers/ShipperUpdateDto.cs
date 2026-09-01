using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Shippers;

// DTO payload for updating an existing Shipper
public class ShipperUpdateDto
{
    [Required(ErrorMessage = "اسم شركة الشحن مطلوب")]
    [StringLength(50, ErrorMessage = "اسم شركة الشحن يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}