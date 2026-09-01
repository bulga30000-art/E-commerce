
using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly StoreContext _context;

    public CategoryRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(byte id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.AsNoTracking().ToListAsync();
    }

    public IQueryable<Category> GetQueryable()
    {
        return _context.Categories.AsQueryable().AsNoTracking();
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        _context.Categories.Remove(category);
    }
}