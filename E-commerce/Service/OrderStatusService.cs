using E_commerce.DTOs.Common;
using E_commerce.DTOs.OrderStatuses;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OrderStatusReadDto>> GetPagedAsync(OrderStatusQueryParams queryParams)
    {
        var query = _unitOfWork.OrderStatuses.GetQueryable();

        // Sort by Name field
        query = queryParams.SortDescending
            ? query.OrderByDescending(os => os.Name)
            : query.OrderBy(os => os.Name);

        var projectedQuery = query.Select(os => new OrderStatusReadDto
        {
            OrderStatusId = os.OrderStatusId,
            Name = os.Name
        });

        return await PagedResult<OrderStatusReadDto>.CreateAsync(
            projectedQuery,
            queryParams.PageNumber,
            queryParams.PageSize);
    }

    public async Task<OrderStatusReadDto> GetByIdAsync(byte id)
    {
        var orderStatus = await _unitOfWork.OrderStatuses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(OrderStatus), id);

        return new OrderStatusReadDto
        {
            OrderStatusId = orderStatus.OrderStatusId,
            Name = orderStatus.Name
        };
    }

    public async Task<OrderStatusReadDto> CreateAsync(OrderStatusCreateDto dto)
    {
        var orderStatus = new OrderStatus
        {
            Name = dto.Name
        };

        await _unitOfWork.OrderStatuses.AddAsync(orderStatus);
        await _unitOfWork.SaveChangesAsync();

        return new OrderStatusReadDto
        {
            OrderStatusId = orderStatus.OrderStatusId,
            Name = orderStatus.Name
        };
    }

    public async Task<OrderStatusReadDto> UpdateAsync(byte id, OrderStatusUpdateDto dto)
    {
        var orderStatus = await _unitOfWork.OrderStatuses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(OrderStatus), id);

        orderStatus.Name = dto.Name;

        _unitOfWork.OrderStatuses.Update(orderStatus);
        await _unitOfWork.SaveChangesAsync();

        return new OrderStatusReadDto
        {
            OrderStatusId = orderStatus.OrderStatusId,
            Name = orderStatus.Name
        };
    }

    public async Task DeleteAsync(byte id)
    {
        var orderStatus = await _unitOfWork.OrderStatuses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(OrderStatus), id);

        // Verify foreign key constraint before deletion to prevent raw DbUpdateException database errors
        var hasOrders = await _unitOfWork.Orders.GetQueryable()
            .AnyAsync(o => o.OrderStatusId == id);

        if (hasOrders)
        {
            throw new ConflictException("لا يمكن حذف حالة الطلب لأنها مرتبطة بطلبات موجودة بالفعل");
        }

        _unitOfWork.OrderStatuses.Delete(orderStatus);
        await _unitOfWork.SaveChangesAsync();
    }
}