using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Categories;

// DTO payload for creating a new Category
public class CategoryCreateDto
{
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [StringLength(50, ErrorMessage = "اسم التصنيف يجب ألا يتجاوز {1} حرف")]
    public string Name { get; set; } = null!;
}