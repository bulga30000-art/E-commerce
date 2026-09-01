using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IShipperRepository
{
    Task<Shipper?> GetByIdAsync(int id);
    Task<IEnumerable<Shipper>> GetAllAsync();
    IQueryable<Shipper> GetQueryable();

    Task AddAsync(Shipper shipper);
    void Update(Shipper shipper);
    void Delete(Shipper shipper);
}