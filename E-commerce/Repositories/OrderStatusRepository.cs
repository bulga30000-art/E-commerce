using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class OrderStatusRepository : IOrderStatusRepository
{
    private readonly StoreContext _context;

    public OrderStatusRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<OrderStatus?> GetByIdAsync(byte id)
    {
        return await _context.OrderStatuses.FindAsync(id);
    }

    public async Task<IEnumerable<OrderStatus>> GetAllAsync()
    {
        return await _context.OrderStatuses.AsNoTracking().ToListAsync();
    }

    public IQueryable<OrderStatus> GetQueryable()
    {
        return _context.OrderStatuses.AsQueryable().AsNoTracking();
    }

    public async Task AddAsync(OrderStatus orderStatus)
    {
        await _context.OrderStatuses.AddAsync(orderStatus);
    }

    public void Update(OrderStatus orderStatus)
    {
        _context.OrderStatuses.Update(orderStatus);
    }

    public void Delete(OrderStatus orderStatus)
    {
        _context.OrderStatuses.Remove(orderStatus);
    }
}