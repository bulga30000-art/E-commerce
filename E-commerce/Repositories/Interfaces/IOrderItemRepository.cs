using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IOrderItemRepository
{
    // Fetches OrderItem by composite primary key (orderId, productId)
    Task<OrderItem?> GetByIdAsync(int orderId, int productId);

    Task AddAsync(OrderItem orderItem);
    void Update(OrderItem orderItem);
    void Delete(OrderItem orderItem);
}