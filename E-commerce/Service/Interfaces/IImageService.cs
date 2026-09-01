namespace E_commerce.Services.Interfaces;

// Abstraction for managing physical image files on disk (saving and deleting).
// Completely decoupled from database entities for reuse across domain modules.
public interface IImageService
{
    // Saves an uploaded file to the specified web root subfolder (e.g., "products")
    // and returns the relative URL path to be stored in the database.
    Task<string> SaveImageAsync(IFormFile file, string subfolder);

    // Deletes an image file from disk using its relative URL path.
    // Handles null or empty paths gracefully for entities without images.
    void DeleteImage(string? relativeImagePath);
}