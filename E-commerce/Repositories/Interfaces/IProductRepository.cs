using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IProductRepository
{
    // Fetches a single product by Id without navigation properties.
    Task<Product?> GetByIdAsync(int id);

    // Fetches a single product by Id including navigation details (e.g. Category).
    Task<Product?> GetByIdWithDetailsAsync(int id);

    // Fetches all products as a list (for simple unpaginated queries).
    Task<IEnumerable<Product>> GetAllAsync();

    // Fetches multiple products by IDs (tracked entities for updating stock in checkout flow).
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);

    // Returns IQueryable for building custom filtering, sorting, and pagination.
    IQueryable<Product> GetQueryable();

    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);

    // Atomic database stock decrement (UPDATE products SET quantity_in_stock = quantity_in_stock - @quantity WHERE product_id = @id AND quantity_in_stock >= @quantity).
    // Executes atomically in SQL Server to eliminate race conditions under concurrent checkout requests.
    // Returns true if stock was decremented successfully, false if stock was insufficient.
    Task<bool> TryDecrementStockAsync(int productId, int quantity);

    // Atomic database stock increment (UPDATE products SET quantity_in_stock = quantity_in_stock + @quantity WHERE product_id = @id).
    // Used during order cancellation to restore stock safely directly in the database.
    Task<bool> IncrementStockAsync(int productId, int quantity);
}