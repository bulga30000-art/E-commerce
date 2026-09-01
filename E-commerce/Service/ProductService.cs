using E_commerce.DTOs.Common;
using E_commerce.DTOs.Products;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using E_commerce.Settings;

namespace E_commerce.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    // Inject UnitOfWork and ImageService dependencies
    public ProductService(IUnitOfWork unitOfWork, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<PagedResult<ProductReadDto>> GetPagedAsync(ProductQueryParams queryParams)
    {
        var query = _unitOfWork.Products.GetQueryable();

        // 1. Filtering (Search by term)
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        // 2. Sorting (by name, price, or default ProductId)
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" => queryParams.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            "price" => queryParams.SortDescending
                ? query.OrderByDescending(p => p.UnitPrice)
                : query.OrderBy(p => p.UnitPrice),

            null => query.OrderBy(p => p.ProductId),

            _ => throw new BadRequestException(
                $"الترتيب غير مسموح به. القيم المتاحة: name, price")
        };

        // 3. DTO Projection
        var projectedQuery = query.Select(p => new ProductReadDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            QuantityInStock = p.QuantityInStock,
            UnitPrice = p.UnitPrice,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name
        });

        // 4. Execution & Pagination
        return await PagedResult<ProductReadDto>.CreateAsync(
            projectedQuery,
            queryParams.PageNumber,
            queryParams.PageSize);
    }

    public async Task<ProductReadDto> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);

        return MapToReadDto(product);
    }

    public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto)
    {
        // Verify target Category exists
        var categoryExists = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (categoryExists is null)
        {
            throw new BadRequestException($"لا يوجد تصنيف بالرقم '{dto.CategoryId}'");
        }

        // Save physical image file first, obtaining relative path
        var imagePath = await _imageService.SaveImageAsync(dto.ImageFile, FileSettings.ProductImagesPath.TrimStart('/'));

        var product = new Product
        {
            Name = dto.Name,
            QuantityInStock = dto.QuantityInStock,
            UnitPrice = dto.UnitPrice,
            CategoryId = dto.CategoryId,
            ImageUrl = imagePath
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(product.ProductId);
    }

    public async Task<ProductReadDto> UpdateAsync(int id, ProductUpdateDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);

        var categoryExists = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (categoryExists is null)
        {
            throw new BadRequestException($"لا يوجد تصنيف بالرقم '{dto.CategoryId}'");
        }

        // Replace existing image if a new image file is provided
        if (dto.ImageFile is not null)
        {
            var newImagePath = await _imageService.SaveImageAsync(dto.ImageFile, FileSettings.ProductImagesPath.TrimStart('/'));

            _imageService.DeleteImage(product.ImageUrl);

            product.ImageUrl = newImagePath;
        }

        product.Name = dto.Name;
        product.QuantityInStock = dto.QuantityInStock;
        product.UnitPrice = dto.UnitPrice;
        product.CategoryId = dto.CategoryId;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(product.ProductId);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Product), id);

        // Remove associated image file from disk before deleting entity
        _imageService.DeleteImage(product.ImageUrl);

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync();
    }

    // Private helper mapping Product entity to ProductReadDto
    private static ProductReadDto MapToReadDto(Product product)
    {
        return new ProductReadDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            QuantityInStock = product.QuantityInStock,
            UnitPrice = product.UnitPrice,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name
        };
    }
}