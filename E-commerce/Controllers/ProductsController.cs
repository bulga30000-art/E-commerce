using E_commerce.DTOs.Common;
using E_commerce.DTOs.Products;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET api/products?pageNumber=1&pageSize=10&searchTerm=phone&sortBy=price&sortDescending=true
    // Public endpoint allowing anonymous users to browse products
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductReadDto>>> GetProducts(
        [FromQuery] ProductQueryParams queryParams)
    {
        var result = await _productService.GetPagedAsync(queryParams);
        return Ok(result);
    }

    // GET api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductReadDto>> GetProduct(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return Ok(result);
    }

    // POST api/products - Requires multipart/form-data payload due to IFormFile image upload
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductReadDto>> CreateProduct([FromForm] ProductCreateDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result);
    }

    // PUT api/products/5
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductReadDto>> UpdateProduct(int id, [FromForm] ProductUpdateDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        return Ok(result);
    }

    // DELETE api/products/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}