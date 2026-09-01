using E_commerce.DTOs.Categories;
using E_commerce.DTOs.Common;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CategoryReadDto>> GetPagedAsync(CategoryQueryParams queryParams)
    {
        var query = _unitOfWork.Categories.GetQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        // Sort by Name field
        query = queryParams.SortDescending
            ? query.OrderByDescending(c => c.Name)
            : query.OrderBy(c => c.Name);

        var projectedQuery = query.Select(c => new CategoryReadDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name
        });

        return await PagedResult<CategoryReadDto>.CreateAsync(
            projectedQuery,
            queryParams.PageNumber,
            queryParams.PageSize);
    }

    public async Task<CategoryReadDto> GetByIdAsync(byte id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);

        return new CategoryReadDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }

    public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // EF Core auto-populates the generated CategoryId on the entity after SaveChangesAsync
        return new CategoryReadDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }

    public async Task<CategoryReadDto> UpdateAsync(byte id, CategoryUpdateDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);

        category.Name = dto.Name;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return new CategoryReadDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };
    }

    public async Task DeleteAsync(byte id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Category), id);

        // Verify foreign key constraint before deletion to prevent raw DbUpdateException database errors
        var hasProducts = await _unitOfWork.Products.GetQueryable()
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            throw new ConflictException("لا يمكن حذف التصنيف لأنه مرتبط بمنتجات موجودة بالفعل");
        }

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();
    }
}