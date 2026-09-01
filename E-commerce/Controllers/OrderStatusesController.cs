using E_commerce.DTOs.Common;
using E_commerce.DTOs.OrderStatuses;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStatusesController : ControllerBase
{
    private readonly IOrderStatusService _orderStatusService;

    public OrderStatusesController(IOrderStatusService orderStatusService)
    {
        _orderStatusService = orderStatusService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderStatusReadDto>>> GetOrderStatuses(
        [FromQuery] OrderStatusQueryParams queryParams)
    {
        var result = await _orderStatusService.GetPagedAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderStatusReadDto>> GetOrderStatus(byte id)
    {
        var result = await _orderStatusService.GetByIdAsync(id);
        return Ok(result);
    }

    // Accepts application/json payload for order status creation
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<OrderStatusReadDto>> CreateOrderStatus(OrderStatusCreateDto dto)
    {
        var result = await _orderStatusService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetOrderStatus), new { id = result.OrderStatusId }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<OrderStatusReadDto>> UpdateOrderStatus(byte id, OrderStatusUpdateDto dto)
    {
        var result = await _orderStatusService.UpdateAsync(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderStatus(byte id)
    {
        await _orderStatusService.DeleteAsync(id);
        return NoContent();
    }
}