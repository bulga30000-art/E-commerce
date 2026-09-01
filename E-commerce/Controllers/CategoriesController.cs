using E_commerce.DTOs.Categories;
using E_commerce.DTOs.Common;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryReadDto>>> GetCategories(
        [FromQuery] CategoryQueryParams queryParams)
    {
        var result = await _categoryService.GetPagedAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategory(byte id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }

    // Accepts application/json payload for category creation
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryReadDto>> CreateCategory(CategoryCreateDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = result.CategoryId }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryReadDto>> UpdateCategory(byte id, CategoryUpdateDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(byte id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}