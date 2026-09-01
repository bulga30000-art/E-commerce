using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface ICustomerRepository
{
    // Retrieves the Customer domain profile linked to the ASP.NET Identity User ID
    Task<Customer?> GetByUserIdAsync(string userId);

    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    IQueryable<Customer> GetQueryable();

    Task AddAsync(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);
}