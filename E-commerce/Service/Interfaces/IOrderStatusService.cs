using E_commerce.DTOs.Common;
using E_commerce.DTOs.OrderStatuses;

namespace E_commerce.Services.Interfaces;

public interface IOrderStatusService
{
    Task<PagedResult<OrderStatusReadDto>> GetPagedAsync(OrderStatusQueryParams queryParams);

    Task<OrderStatusReadDto> GetByIdAsync(byte id);

    Task<OrderStatusReadDto> CreateAsync(OrderStatusCreateDto dto);

    Task<OrderStatusReadDto> UpdateAsync(byte id, OrderStatusUpdateDto dto);

    Task DeleteAsync(byte id);
}