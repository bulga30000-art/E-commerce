using E_commerce.DTOs.Common;
using E_commerce.DTOs.Shippers;

namespace E_commerce.Services.Interfaces;

public interface IShipperService
{
    Task<PagedResult<ShipperReadDto>> GetPagedAsync(ShipperQueryParams queryParams);

    Task<ShipperReadDto> GetByIdAsync(int id);

    Task<ShipperReadDto> CreateAsync(ShipperCreateDto dto);

    Task<ShipperReadDto> UpdateAsync(int id, ShipperUpdateDto dto);

    Task DeleteAsync(int id);
}