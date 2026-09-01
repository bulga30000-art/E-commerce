// Repositories/Interfaces/IOrderStatusRepository.cs
using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IOrderStatusRepository
{
    Task<OrderStatus?> GetByIdAsync(byte id);
    Task<IEnumerable<OrderStatus>> GetAllAsync();
    IQueryable<OrderStatus> GetQueryable();

    Task AddAsync(OrderStatus orderStatus);
    void Update(OrderStatus orderStatus);
    void Delete(OrderStatus orderStatus);
}