// Repositories/OrderItemRepository.cs
using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreContext _context;

    public OrderItemRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<OrderItem?> GetByIdAsync(int orderId, int productId)
    {
        // FindAsync accepts multiple key values for composite primary keys.
        // Order of parameters must match composite key ordering defined in StoreContext model creation.
        return await _context.OrderItems.FindAsync(orderId, productId);
    }

    public async Task AddAsync(OrderItem orderItem)
    {
        await _context.OrderItems.AddAsync(orderItem);
    }

    public void Update(OrderItem orderItem)
    {
        _context.OrderItems.Update(orderItem);
    }

    public void Delete(OrderItem orderItem)
    {
        _context.OrderItems.Remove(orderItem);
    }
}