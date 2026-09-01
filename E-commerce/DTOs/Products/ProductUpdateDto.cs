using System.ComponentModel.DataAnnotations;
using E_commerce.Settings;
using E_commerce.Validation;

namespace E_commerce.DTOs.Products;

// DTO payload for updating an existing product (ImageFile is optional to allow preserving current image)
public class ProductUpdateDto
{
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(100, ErrorMessage = "اسم المنتج يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;

    [Range(0, int.MaxValue, ErrorMessage = "الكمية في المخزون يجب ألا تكون سالبة")]
    public int QuantityInStock { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "سعر الوحدة يجب أن يكون بين {1} و {2}")]
    public decimal UnitPrice { get; set; }

    // Optional image file replacement (if null, existing image is retained)
    [AllowedExtensions(FileSettings.AllowedExtensions)]
    [MaxFileSize(FileSettings.MaxFileSizeInBytes)]
    public IFormFile? ImageFile { get; set; }

    public byte CategoryId { get; set; }
}