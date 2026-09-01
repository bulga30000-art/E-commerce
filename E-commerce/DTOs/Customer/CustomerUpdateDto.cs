using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Customer;

// DTO payload for updating customer profile details
public class CustomerUpdateDto
{
    [Required(ErrorMessage = "الاسم الاول مطلوب")]
    [StringLength(50, ErrorMessage = "الاسم الاول يجب ألا يتجاوز {1} حرف")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "الاسم الثاني مطلوب")]
    [StringLength(50, ErrorMessage = "الاسم الثاني يجب ألا يتجاوز {1} حرف")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
    [StringLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز {1} حرف")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(100, ErrorMessage = "العنوان يجب ألا يتجاوز {1} حرف")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "المدينة مطلوبة")]
    [StringLength(50, ErrorMessage = "المدينة يجب ألا تتجاوز {1} حرف")]
    public string City { get; set; } = null!;
}