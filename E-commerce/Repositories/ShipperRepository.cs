using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class ShipperRepository : IShipperRepository
{
    private readonly StoreContext _context;

    public ShipperRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<Shipper?> GetByIdAsync(int id)
    {
        return await _context.Shippers.FindAsync(id);
    }

    public async Task<IEnumerable<Shipper>> GetAllAsync()
    {
        return await _context.Shippers.AsNoTracking().ToListAsync();
    }

    public IQueryable<Shipper> GetQueryable()
    {
        return _context.Shippers.AsQueryable().AsNoTracking();
    }

    public async Task AddAsync(Shipper shipper)
    {
        await _context.Shippers.AddAsync(shipper);
    }

    public void Update(Shipper shipper)
    {
        _context.Shippers.Update(shipper);
    }

    public void Delete(Shipper shipper)
    {
        _context.Shippers.Remove(shipper);
    }
}