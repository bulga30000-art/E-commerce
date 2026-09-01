using System.ComponentModel.DataAnnotations;
using E_commerce.Settings;
using E_commerce.Validation;

namespace E_commerce.DTOs.Products;

// DTO payload for creating a new product
public class ProductCreateDto
{
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(100, ErrorMessage = "اسم المنتج يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;

    // Enforce non-negative stock quantity
    [Range(0, int.MaxValue, ErrorMessage = "الكمية في المخزون يجب ألا تكون سالبة")]
    public int QuantityInStock { get; set; }

    // Enforce valid positive price range matching database decimal(8,2) precision
    [Range(0.01, 999999.99, ErrorMessage = "سعر الوحدة يجب أن يكون بين {1} و {2}")]
    public decimal UnitPrice { get; set; }

    // Required image file upload via multipart/form-data with extension and size validation attributes
    [Required(ErrorMessage = "صورة المنتج مطلوبة")]
    [AllowedExtensions(FileSettings.AllowedExtensions)]
    [MaxFileSize(FileSettings.MaxFileSizeInBytes)]
    public IFormFile ImageFile { get; set; } = null!;

    // Category ID foreign key (existence verified at Service Layer)
    public byte CategoryId { get; set; }
}