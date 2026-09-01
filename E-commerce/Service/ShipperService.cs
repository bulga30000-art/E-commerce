using E_commerce.DTOs.Common;
using E_commerce.DTOs.Shippers;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services;

public class ShipperService : IShipperService
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipperService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ShipperReadDto>> GetPagedAsync(ShipperQueryParams queryParams)
    {
        var query = _unitOfWork.Shippers.GetQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term));
        }

        // Sort by Name field
        query = queryParams.SortDescending
            ? query.OrderByDescending(s => s.Name)
            : query.OrderBy(s => s.Name);

        var projectedQuery = query.Select(s => new ShipperReadDto
        {
            ShipperId = s.ShipperId,
            Name = s.Name
        });

        return await PagedResult<ShipperReadDto>.CreateAsync(
            projectedQuery,
            queryParams.PageNumber,
            queryParams.PageSize);
    }

    public async Task<ShipperReadDto> GetByIdAsync(int id)
    {
        var shipper = await _unitOfWork.Shippers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shipper), id);

        return new ShipperReadDto
        {
            ShipperId = shipper.ShipperId,
            Name = shipper.Name
        };
    }

    public async Task<ShipperReadDto> CreateAsync(ShipperCreateDto dto)
    {
        var shipper = new Shipper
        {
            Name = dto.Name
        };

        await _unitOfWork.Shippers.AddAsync(shipper);
        await _unitOfWork.SaveChangesAsync();

        return new ShipperReadDto
        {
            ShipperId = shipper.ShipperId,
            Name = shipper.Name
        };
    }

    public async Task<ShipperReadDto> UpdateAsync(int id, ShipperUpdateDto dto)
    {
        var shipper = await _unitOfWork.Shippers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shipper), id);

        shipper.Name = dto.Name;

        _unitOfWork.Shippers.Update(shipper);
        await _unitOfWork.SaveChangesAsync();

        return new ShipperReadDto
        {
            ShipperId = shipper.ShipperId,
            Name = shipper.Name
        };
    }

    public async Task DeleteAsync(int id)
    {
        var shipper = await _unitOfWork.Shippers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Shipper), id);

        // Verify foreign key constraint before deletion to prevent raw DbUpdateException database errors
        var hasOrders = await _unitOfWork.Orders.GetQueryable()
            .AnyAsync(o => o.ShipperId == id);

        if (hasOrders)
        {
            throw new ConflictException("لا يمكن حذف شركة الشحن لأنها مرتبطة بطلبات موجودة بالفعل");
        }

        _unitOfWork.Shippers.Delete(shipper);
        await _unitOfWork.SaveChangesAsync();
    }
}