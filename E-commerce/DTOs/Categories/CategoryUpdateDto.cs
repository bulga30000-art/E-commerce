using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Categories;

// DTO payload for updating an existing Category
public class CategoryUpdateDto
{
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [StringLength(50, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}