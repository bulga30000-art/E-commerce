namespace E_commerce.Settings;

// Centralized image upload settings for allowed file formats and maximum file size.
public static class FileSettings
{
    // Relative storage directory path under wwwroot for product images
    public const string ProductImagesPath = "/images/products";

    public const string AllowedExtensions = ".jpg,.jpeg,.png,.webp";

    public const int MaxFileSizeInMB = 2;
    public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
}