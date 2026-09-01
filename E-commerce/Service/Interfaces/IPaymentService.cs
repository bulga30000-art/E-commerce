using E_commerce.DTOs.Payments;

namespace E_commerce.Services.Interfaces;

public interface IPaymentService
{
    // Initiates payment for an order. CreditCard payments complete immediately;
    // Cash payments remain Pending until admin confirmation.
    Task<PaymentReadDto> CreatePaymentAsync(int orderId, int customerId, PaymentCreateDto dto);

    // Retrieves payment details for a specific order. Verifies caller ownership unless admin.
    Task<PaymentReadDto> GetByOrderIdAsync(int orderId, int requestingCustomerId, bool isAdmin);

    // Admin endpoint to confirm or reject a pending Cash payment.
    Task<PaymentReadDto> UpdatePaymentStatusAsync(int orderId, PaymentStatusUpdateDto dto);
}