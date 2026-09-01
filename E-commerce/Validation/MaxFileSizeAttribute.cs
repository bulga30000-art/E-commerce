using System.ComponentModel.DataAnnotations;

namespace E_commerce.Validation;

// Custom Data Annotation Attribute to validate uploaded file size against a maximum threshold.
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSizeInBytes;

    public MaxFileSizeAttribute(int maxFileSizeInBytes)
    {
        _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;

        // Inspect file byte length directly using IFormFile.Length
        if (file is not null && file.Length > _maxFileSizeInBytes)
        {
            return new ValidationResult($"أقصى حجم مسموح به للصورة هو {_maxFileSizeInBytes / (1024 * 1024)} ميجابايت");
        }

        return ValidationResult.Success;
    }
}