using E_commerce.DTOs.Common;
using E_commerce.DTOs.Orders;

namespace E_commerce.Services.Interfaces;

public interface IOrderService
{
    // Complete order checkout. The customerId is retrieved directly from JWT claims
    // to guarantee customers can only create orders for themselves.
    Task<OrderReadDto> CheckoutAsync(int customerId, OrderCreateDto dto);

    // Retrieves full details for a single order. Verifies ownership if caller is non-admin.
    Task<OrderReadDto> GetByIdAsync(int orderId, int requestingCustomerId, bool isAdmin);

    // Administrative order status update enforcing valid state transitions (State Machine).
    Task<OrderReadDto> UpdateStatusAsync(int orderId, byte newStatusId);

    // Cancels an order. Customers can only cancel their own Pending orders;
    // Admins can cancel any order that is still in an early stage (Pending/Processing).
    Task<OrderReadDto> CancelOrderAsync(int orderId, int requestingCustomerId, bool isAdmin);

    // Attempts to transition order status to Processing automatically upon successful payment.
    // Operates on a Best-Effort basis and ignores calls if the order is no longer Pending.
    Task TryMarkProcessingAsync(int orderId);

    // Retrieves paginated orders owned by the authenticated customer ("My Orders").
    Task<PagedResult<OrderSummaryReadDto>> GetMyOrdersAsync(int customerId, OrderQueryParams queryParams);

    // Administrative listing of all system orders with pagination, filtering, and sorting.
    Task<PagedResult<OrderSummaryReadDto>> GetAllAsync(OrderQueryParams queryParams);
}