using System.ComponentModel.DataAnnotations;

namespace E_commerce.Validation;

// Custom Data Annotation Attribute extending ValidationAttribute to validate file extensions
// (e.g., [AllowedExtensions(FileSettings.AllowedExtensions)]).
public class AllowedExtensionsAttribute : ValidationAttribute
{
    // Allowed extensions passed from FileSettings or attribute instantiation
    private readonly string _allowedExtensions;

    public AllowedExtensionsAttribute(string allowedExtensions)
    {
        _allowedExtensions = allowedExtensions;
    }

    // Executed automatically during model state validation
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Cast input object to IFormFile
        var file = value as IFormFile;

        // If file is null, defer null checking to [Required] attribute if present
        if (file is not null)
        {
            var extension = Path.GetExtension(file.FileName);

            var isAllowed = _allowedExtensions.Split(',')
                .Contains(extension, StringComparer.OrdinalIgnoreCase);

            if (!isAllowed)
            {
                return new ValidationResult($"صيغ الصور المسموح بيها بس: {_allowedExtensions}");
            }
        }

        return ValidationResult.Success;
    }
}