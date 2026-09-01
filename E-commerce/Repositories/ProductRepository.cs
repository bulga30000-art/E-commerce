using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly StoreContext _context;

    public ProductRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.AsNoTracking().ToListAsync();
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        // Intentionally tracked (without AsNoTracking) so changes to QuantityInStock
        // are tracked by EF Core ChangeTracker and saved via UnitOfWork.SaveChangesAsync().
        return await _context.Products
            .Where(p => ids.Contains(p.ProductId))
            .ToListAsync();
    }

    public IQueryable<Product> GetQueryable()
    {
        // Returns IQueryable without execution (AsNoTracking) for deferred evaluation in services.
        return _context.Products.AsQueryable().AsNoTracking();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Delete(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task<bool> TryDecrementStockAsync(int productId, int quantity)
    {
        // ExecuteUpdateAsync issues a direct SQL UPDATE statement bypassing EF Core Change Tracker.
        // This ensures atomic execution at the database level: the WHERE condition (QuantityInStock >= quantity)
        // and deduction occur within a single SQL statement locked by SQL Server.
        // If 0 rows are affected, returns false (insufficient stock or race condition) allowing service to rollback.
        var affectedRows = await _context.Products
            .Where(p => p.ProductId == productId && p.QuantityInStock >= quantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.QuantityInStock, p => p.QuantityInStock - quantity));

        return affectedRows > 0;
    }

    public async Task<bool> IncrementStockAsync(int productId, int quantity)
    {
        // Direct SQL update to restore stock (e.g. on order cancellation) atomically.
        var affectedRows = await _context.Products
            .Where(p => p.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.QuantityInStock, p => p.QuantityInStock + quantity));

        return affectedRows > 0;
    }
}