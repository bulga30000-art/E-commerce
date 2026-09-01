using System.ComponentModel.DataAnnotations;

namespace E_commerce.DTOs.Auth;

// DTO payload for new user registration
public class RegisterDto
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [StringLength(256, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز {1} حرف")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(15, MinimumLength = 6,
        ErrorMessage = "كلمة المرور يجب أن تكون بين {2} و {1} حرف")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [StringLength(50, ErrorMessage = "الاسم الأول يجب ألا يتجاوز {1} حرف")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "اسم العائلة مطلوب")]
    [StringLength(50, ErrorMessage = "اسم العائلة يجب ألا يتجاوز {1} حرف")]
    public string LastName { get; set; } = null!;

    [StringLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز {1} رقم")]
    [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(100, ErrorMessage = "العنوان يجب ألا يتجاوز {1} حرف")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "المدينة مطلوبة")]
    [StringLength(50, ErrorMessage = "المدينة يجب ألا يتجاوز {1} حرف")]
    public string City { get; set; } = null!;
}