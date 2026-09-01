using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly StoreContext _context;

    public CustomerRepository(StoreContext context)
    {
        _context = context;
    }

    // Fetches Customer entity matching the ASP.NET Identity User ID
    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
    }
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers.AsNoTracking().ToListAsync();
    }

    public IQueryable<Customer> GetQueryable()
    {
        return _context.Customers.AsQueryable().AsNoTracking();
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public void Delete(Customer customer)
    {
        _context.Customers.Remove(customer);
    }
}