using E_commerce.DTOs.Common;
using E_commerce.DTOs.Products;

namespace E_commerce.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductReadDto>> GetPagedAsync(ProductQueryParams queryParams);

    Task<ProductReadDto> GetByIdAsync(int id);

    Task<ProductReadDto> CreateAsync(ProductCreateDto dto);

    Task<ProductReadDto> UpdateAsync(int id, ProductUpdateDto dto);

    Task DeleteAsync(int id);
}