using E_commerce.DTOs.Payments;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/orders/{orderId}/payment")]
[Authorize] // All payment endpoints require authentication
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // POST api/orders/{orderId}/payment - Initiate payment for an order (Customer role required)
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<PaymentReadDto>> Pay(int orderId, PaymentCreateDto dto)
    {
        var customerId = GetCustomerId();
        var result = await _paymentService.CreatePaymentAsync(orderId, customerId, dto);
        return CreatedAtAction(nameof(GetPayment), new { orderId }, result);
    }

    // GET api/orders/{orderId}/payment - Retrieve payment details for an order (Owner customer or Admin)
    [HttpGet]
    public async Task<ActionResult<PaymentReadDto>> GetPayment(int orderId)
    {
        var isAdmin = User.IsInRole("Admin");
        var customerId = GetCustomerId();

        var result = await _paymentService.GetByOrderIdAsync(orderId, customerId, isAdmin);
        return Ok(result);
    }

    // PUT api/orders/{orderId}/payment/status - Admin endpoint to confirm or reject pending Cash payments
    [Authorize(Roles = "Admin")]
    [HttpPut("status")]
    public async Task<ActionResult<PaymentReadDto>> UpdateStatus(int orderId, PaymentStatusUpdateDto dto)
    {
        var result = await _paymentService.UpdatePaymentStatusAsync(orderId, dto);
        return Ok(result);
    }

    // Retrieves customerId from authenticated JWT claims
    private int GetCustomerId()
    {
        var claim = User.FindFirst("customerId")?.Value;
        return int.Parse(claim!);
    }
}