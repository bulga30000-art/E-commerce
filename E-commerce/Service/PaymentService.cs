using E_commerce.Common;
using E_commerce.DTOs.Payments;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;

namespace E_commerce.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    // Relies on IOrderService for triggering order state machine transitions upon payment completion
    private readonly IOrderService _orderService;

    public PaymentService(IUnitOfWork unitOfWork, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
    }

    public async Task<PaymentReadDto> CreatePaymentAsync(int orderId, int customerId, PaymentCreateDto dto)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        // Security check: Validate caller ownership (returns NotFound to avoid revealing order existence)
        if (order.CustomerId != customerId)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        // Prevent payment for cancelled orders
        var currentStatusId = order.OrderStatusId ?? OrderStatusIds.Pending;
        if (currentStatusId == OrderStatusIds.Cancelled)
        {
            throw new ConflictException("لا يمكن الدفع لطلب تم إلغاؤه");
        }

        // Verify that order has no existing payment record
        var existingPayment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        if (existingPayment is not null)
        {
            throw new ConflictException("هذا الطلب لديه عملية دفع مسجلة بالفعل");
        }

        // Validate allowed payment method
        if (dto.PaymentMethod != PaymentMethods.CreditCard && dto.PaymentMethod != PaymentMethods.Cash)
        {
            throw new BadRequestException("طريقة الدفع غير مدعومة. القيم المسموحة: CreditCard أو Cash");
        }

        // CreditCard completes instantly; Cash remains Pending until admin confirmation
        var initialStatus = dto.PaymentMethod == PaymentMethods.CreditCard
            ? PaymentStatuses.Completed
            : PaymentStatuses.Pending;

        var payment = new Payment
        {
            OrderId = orderId,
            PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = order.TotalAmount ?? 0m, // Take order total amount snapshot
            PaymentMethod = dto.PaymentMethod,
            Status = initialStatus
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // If payment completed immediately (CreditCard), attempt to transition order to Processing
        if (payment.Status == PaymentStatuses.Completed)
        {
            await _orderService.TryMarkProcessingAsync(orderId);
        }

        return MapToReadDto(payment);
    }

    public async Task<PaymentReadDto> GetByOrderIdAsync(int orderId, int requestingCustomerId, bool isAdmin)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (!isAdmin && order.CustomerId != requestingCustomerId)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException(nameof(Payment), orderId);

        return MapToReadDto(payment);
    }

    public async Task<PaymentReadDto> UpdatePaymentStatusAsync(int orderId, PaymentStatusUpdateDto dto)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId)
            ?? throw new NotFoundException(nameof(Payment), orderId);

        // Only Cash payments can be updated manually
        if (payment.PaymentMethod != PaymentMethods.Cash)
        {
            throw new ConflictException("لا يمكن تعديل حالة دفعة تمت عن طريق الكارت يدوياً");
        }

        // Prevent modifying finalized payment records
        if (payment.Status != PaymentStatuses.Pending)
        {
            throw new ConflictException($"حالة الدفع الحالية '{payment.Status}' لا تسمح بالتعديل");
        }

        if (dto.NewStatus != PaymentStatuses.Completed && dto.NewStatus != PaymentStatuses.Failed)
        {
            throw new BadRequestException("الحالة الجديدة غير صالحة. القيم المسموحة: Completed أو Failed");
        }

        payment.Status = dto.NewStatus;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        // Attempt transition to Processing if cash payment is confirmed
        if (payment.Status == PaymentStatuses.Completed)
        {
            await _orderService.TryMarkProcessingAsync(payment.OrderId);
        }

        return MapToReadDto(payment);
    }

    private static PaymentReadDto MapToReadDto(Payment payment)
    {
        return new PaymentReadDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod ?? string.Empty,
            Status = payment.Status ?? string.Empty
        };
    }
}