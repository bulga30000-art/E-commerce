using System.Security.Claims;
using E_commerce.DTOs.Common;
using E_commerce.DTOs.Orders;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST api/orders - Checkout creation for authenticated customers
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<OrderReadDto>> Checkout(OrderCreateDto dto)
    {
        var customerId = GetCustomerId();
        var result = await _orderService.CheckoutAsync(customerId, dto);
        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
    }

    // GET api/orders/mine - Paginated list of orders for the currently authenticated customer
    [Authorize(Roles = "Customer")]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<OrderSummaryReadDto>>> GetMyOrders(
        [FromQuery] OrderQueryParams queryParams)
    {
        var customerId = GetCustomerId();
        var result = await _orderService.GetMyOrdersAsync(customerId, queryParams);
        return Ok(result);
    }

    // GET api/orders - Admin listing of all orders across the system
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryReadDto>>> GetAllOrders(
        [FromQuery] OrderQueryParams queryParams)
    {
        var result = await _orderService.GetAllAsync(queryParams);
        return Ok(result);
    }

    // GET api/orders/{id} - Detailed view of a single order. Validates ownership for customers
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderReadDto>> GetOrder(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var customerId = GetCustomerId();

        var result = await _orderService.GetByIdAsync(id, customerId, isAdmin);
        return Ok(result);
    }

    // PUT api/orders/{id}/status - Administrative order status update (State Machine transition)
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderReadDto>> UpdateOrderStatus(int id, OrderStatusChangeDto dto)
    {
        var result = await _orderService.UpdateStatusAsync(id, dto.NewStatusId);
        return Ok(result);
    }

    // POST api/orders/{id}/cancel - Order cancellation endpoint accessible to both Customers and Admins
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderReadDto>> CancelOrder(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var customerId = GetCustomerId();

        var result = await _orderService.CancelOrderAsync(id, customerId, isAdmin);
        return Ok(result);
    }

    // Extracts customerId from JWT claims populated during user authentication
    private int GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return int.Parse(claim!);
    }
}