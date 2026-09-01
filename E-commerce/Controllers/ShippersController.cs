using E_commerce.DTOs.Common;
using E_commerce.DTOs.Shippers;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippersController : ControllerBase
{
    private readonly IShipperService _shipperService;

    public ShippersController(IShipperService shipperService)
    {
        _shipperService = shipperService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ShipperReadDto>>> GetShippers(
        [FromQuery] ShipperQueryParams queryParams)
    {
        var result = await _shipperService.GetPagedAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShipperReadDto>> GetShipper(int id)
    {
        var result = await _shipperService.GetByIdAsync(id);
        return Ok(result);
    }

    // Accepts application/json payload for shipper creation
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ShipperReadDto>> CreateShipper(ShipperCreateDto dto)
    {
        var result = await _shipperService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetShipper), new { id = result.ShipperId }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ShipperReadDto>> UpdateShipper(int id, ShipperUpdateDto dto)
    {
        var result = await _shipperService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShipper(int id)
    {
        await _shipperService.DeleteAsync(id);
        return NoContent();
    }
}