
using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(byte id);   // Primary key type is byte
    Task<IEnumerable<Category>> GetAllAsync();
    IQueryable<Category> GetQueryable();

    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
}