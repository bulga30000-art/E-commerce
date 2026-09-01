using E_commerce.DTOs.Categories;
using E_commerce.DTOs.Common;

namespace E_commerce.Services.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<CategoryReadDto>> GetPagedAsync(CategoryQueryParams queryParams);

    Task<CategoryReadDto> GetByIdAsync(byte id);

    Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto);

    Task<CategoryReadDto> UpdateAsync(byte id, CategoryUpdateDto dto);

    Task DeleteAsync(byte id);
}