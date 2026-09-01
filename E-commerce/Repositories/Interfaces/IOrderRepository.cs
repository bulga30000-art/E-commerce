using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);

    // Fetches full order details including navigation properties (Customer, OrderItems, Payment, Shipper).
    Task<Order?> GetByIdWithDetailsAsync(int id);

    // Tracked entity query specifically for status transitions. Loads OrderItems (for restock on cancellation)
    // and OrderStatus (for state machine validation) while allowing change tracking.
    Task<Order?> GetByIdForStatusChangeAsync(int id);

    Task<IEnumerable<Order>> GetAllAsync();
    IQueryable<Order> GetQueryable();

    // Returns IQueryable of orders for a specific customer ("My Orders").
    IQueryable<Order> GetQueryableByCustomerId(int customerId);

    Task AddAsync(Order order);
    void Update(Order order);
    void Delete(Order order);
}