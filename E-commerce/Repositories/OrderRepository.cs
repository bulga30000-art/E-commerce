using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreContext _context;

    public OrderRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)   // Also load associated Product details for each OrderItem
            .Include(o => o.OrderStatus)
            .Include(o => o.Shipper)
            .Include(o => o.Payment).AsSplitQuery().AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<Order?> GetByIdForStatusChangeAsync(int id)
    {
        // Intentionally tracked query (without AsNoTracking). Loads OrderItems (for restock on cancellation)
        // and OrderStatus (for status transition verification and exception formatting).
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.AsNoTracking().ToListAsync();
    }

    public IQueryable<Order> GetQueryable()
    {
        return _context.Orders.AsQueryable().AsNoTracking();
    }

    public IQueryable<Order> GetQueryableByCustomerId(int customerId)
    {
        return _context.Orders.Where(o => o.CustomerId == customerId).AsNoTracking();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public void Delete(Order order)
    {
        _context.Orders.Remove(order);
    }
}