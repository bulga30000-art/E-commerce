using E_commerce.Services.Interfaces;

namespace E_commerce.Services;

// Implementation of IImageService utilizing IWebHostEnvironment to access physical wwwroot paths.
public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImageService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string subfolder)
    {
        // Generate a unique GUID filename to prevent filename collisions on disk
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        // Target storage folder path under wwwroot (e.g., wwwroot/images/products)
        var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, subfolder);

        // Create target directory if it does not exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fullPath = Path.Combine(folderPath, fileName);

        // Stream file contents safely to disk
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path to be stored in database
        return $"{subfolder}/{fileName}";
    }

    public void DeleteImage(string? relativeImagePath)
    {
        if (string.IsNullOrEmpty(relativeImagePath))
        {
            return;
        }

        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativeImagePath);

        // Delete physical file if it exists on disk
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}