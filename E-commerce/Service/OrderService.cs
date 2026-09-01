using E_commerce.Common;
using E_commerce.DTOs.Common;
using E_commerce.DTOs.Orders;
using E_commerce.Exceptions;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;

namespace E_commerce.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    private const byte InitialOrderStatusId = OrderStatusIds.Pending;

    private const byte StatusProcessing = OrderStatusIds.Processing;
    private const byte StatusShipped = OrderStatusIds.Shipped;
    private const byte StatusDelivered = OrderStatusIds.Delivered;
    private const byte StatusCancelled = OrderStatusIds.Cancelled;

    private static readonly Dictionary<byte, byte[]> AllowedTransitions = new()
    {
        [InitialOrderStatusId] = new byte[] { StatusProcessing, StatusCancelled },
        [StatusProcessing] = new byte[] { StatusShipped, StatusCancelled },
        [StatusShipped] = new byte[] { StatusDelivered },
        [StatusDelivered] = Array.Empty<byte>(),
        [StatusCancelled] = Array.Empty<byte>()
    };

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderReadDto> CheckoutAsync(int customerId, OrderCreateDto dto)
    {
        var duplicateProductId = dto.Items
            .GroupBy(i => i.ProductId)
            .FirstOrDefault(g => g.Count() > 1)?.Key;

        if (duplicateProductId is not null)
        {
            throw new BadRequestException(
                $"لا يمكن تكرار نفس المنتج أكثر من مرة في الطلب الواحد (المنتج رقم {duplicateProductId} مكرر)");
        }

        var requestedProductIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _unitOfWork.Products.GetByIdsAsync(requestedProductIds);
        var productsById = products.ToDictionary(p => p.ProductId);

        var missingProductIds = requestedProductIds.Distinct()
            .Where(id => !productsById.ContainsKey(id))
            .ToList();

        if (missingProductIds.Count > 0)
        {
            throw new NotFoundException(nameof(Product), missingProductIds[0]);
        }

        foreach (var item in dto.Items)
        {
            var product = productsById[item.ProductId];
            if (product.QuantityInStock < item.Quantity)
            {
                throw new ConflictException(
                    $"الكمية المتاحة من المنتج '{product.Name}' غير كافية " +
                    $"(المتاح: {product.QuantityInStock}, المطلوب: {item.Quantity})");
            }
        }

        if (dto.ShipperId.HasValue)
        {
            var shipper = await _unitOfWork.Shippers.GetByIdAsync(dto.ShipperId.Value)
                ?? throw new NotFoundException(nameof(Shipper), dto.ShipperId.Value);
        }

        Customer? customer = null;
        if (dto.PointsToRedeem > 0)
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new NotFoundException(nameof(Customer), customerId);

            if (customer.Points < dto.PointsToRedeem)
            {
                throw new ConflictException(
                    $"رصيدك الحالي من نقاط الولاء ({customer.Points} نقطة) غير كافٍ " +
                    $"لصرف {dto.PointsToRedeem} نقطة");
            }
        }

        var initialStatus = await _unitOfWork.OrderStatuses.GetByIdAsync(InitialOrderStatusId)
            ?? throw new BadRequestException("حالة الطلب الابتدائية غير معرفة في النظام، برجاء التواصل مع الدعم الفني");

        // Initial optimism stock check was performed above.
        // True race condition protection occurs below via atomic DB decrement (TryDecrementStockAsync) inside a transaction.

        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0m;

        foreach (var item in dto.Items)
        {
            var product = productsById[item.ProductId];

            orderItems.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.UnitPrice
            });

            totalAmount += item.Quantity * product.UnitPrice;
        }

        var pointsRedeemed = 0;
        if (dto.PointsToRedeem > 0)
        {
            var redeemValue = dto.PointsToRedeem * LoyaltyConstants.RedeemPointValue;
            if (redeemValue > totalAmount)
            {
                throw new BadRequestException(
                    $"قيمة النقاط المطلوب صرفها ({redeemValue:0.00} جنيه) أكبر من إجمالي الطلب ({totalAmount:0.00} جنيه)");
            }

            totalAmount -= redeemValue;
            pointsRedeemed = dto.PointsToRedeem;
        }

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
            OrderStatusId = InitialOrderStatusId,
            Comments = dto.Comments,
            ShipperId = dto.ShipperId,
            TotalAmount = totalAmount,
            PointsRedeemed = pointsRedeemed,
            PointsEarned = 0,
            OrderItems = orderItems
        };

        // ------------------- Critical Section -------------------
        // All steps must complete atomically (atomic stock decrement + points redemption + order save)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in dto.Items)
            {
                var stockDecremented = await _unitOfWork.Products.TryDecrementStockAsync(
                    item.ProductId, item.Quantity);

                if (!stockDecremented)
                {
                    var product = productsById[item.ProductId];
                    throw new ConflictException(
                        $"الكمية المتاحة من المنتج '{product.Name}' غير كافية، برجاء المحاولة مرة أخرى");
                }
            }

            if (pointsRedeemed > 0)
            {
                customer!.Points -= pointsRedeemed;
                _unitOfWork.Customers.Update(customer);
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        // --------------------------------------------------------

        return new OrderReadDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            OrderStatusId = initialStatus.OrderStatusId,
            OrderStatusName = initialStatus.Name,
            Comments = order.Comments,
            ShippedDate = order.ShippedDate,
            ShipperId = order.ShipperId,
            ShipperName = null,
            TotalAmount = order.TotalAmount ?? 0m,
            PointsRedeemed = order.PointsRedeemed,
            PointsEarned = order.PointsEarned,
            Items = orderItems.Select(oi => new OrderItemReadDto
            {
                ProductId = oi.ProductId,
                ProductName = productsById[oi.ProductId].Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }

    public async Task<OrderReadDto> GetByIdAsync(int orderId, int requestingCustomerId, bool isAdmin)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (!isAdmin && order.CustomerId != requestingCustomerId)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        return MapToReadDto(order);
    }

    public async Task<OrderReadDto> UpdateStatusAsync(int orderId, byte newStatusId)
    {
        var order = await _unitOfWork.Orders.GetByIdForStatusChangeAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        var currentStatusId = order.OrderStatusId ?? InitialOrderStatusId;

        // Transaction ensures order status update, stock restoration (if cancelling), and loyalty point restoration execute atomically
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await ApplyStatusTransitionAsync(order, currentStatusId, newStatusId);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return await GetByIdAsync(orderId, order.CustomerId, isAdmin: true);
    }

    public async Task<OrderReadDto> CancelOrderAsync(int orderId, int requestingCustomerId, bool isAdmin)
    {
        var order = await _unitOfWork.Orders.GetByIdForStatusChangeAsync(orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (!isAdmin && order.CustomerId != requestingCustomerId)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        var currentStatusId = order.OrderStatusId ?? InitialOrderStatusId;

        if (!isAdmin && currentStatusId != InitialOrderStatusId)
        {
            throw new ConflictException("لا يمكن إلغاء الطلب بعد بدء تجهيزه، برجاء التواصل مع خدمة العملاء");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await ApplyStatusTransitionAsync(order, currentStatusId, StatusCancelled);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return await GetByIdAsync(orderId, order.CustomerId, isAdmin: true);
    }

    public async Task<PagedResult<OrderSummaryReadDto>> GetMyOrdersAsync(int customerId, OrderQueryParams queryParams)
    {
        var query = _unitOfWork.Orders.GetQueryableByCustomerId(customerId);
        return await BuildPagedSummaryAsync(query, queryParams);
    }

    public async Task<PagedResult<OrderSummaryReadDto>> GetAllAsync(OrderQueryParams queryParams)
    {
        var query = _unitOfWork.Orders.GetQueryable();
        return await BuildPagedSummaryAsync(query, queryParams);
    }

    public async Task TryMarkProcessingAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdForStatusChangeAsync(orderId);
        if (order is null)
        {
            return;
        }

        var currentStatusId = order.OrderStatusId ?? InitialOrderStatusId;
        if (currentStatusId != InitialOrderStatusId)
        {
            return;
        }

        try
        {
            await ApplyStatusTransitionAsync(order, currentStatusId, StatusProcessing);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (ConflictException)
        {
        }
    }

    // ------------------- Private Helpers -------------------

    private async Task ApplyStatusTransitionAsync(Order order, byte currentStatusId, byte newStatusId)
    {
        var newStatus = await _unitOfWork.OrderStatuses.GetByIdAsync(newStatusId)
            ?? throw new NotFoundException(nameof(OrderStatus), newStatusId);

        var allowedNextStatuses = AllowedTransitions.TryGetValue(currentStatusId, out var next)
            ? next
            : Array.Empty<byte>();

        if (!allowedNextStatuses.Contains(newStatusId))
        {
            var currentStatusName = order.OrderStatus?.Name ?? currentStatusId.ToString();
            throw new ConflictException(
                $"لا يمكن تغيير حالة الطلب من '{currentStatusName}' إلى '{newStatus.Name}'");
        }

        if (newStatusId == StatusShipped)
        {
            order.ShippedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        if (newStatusId == StatusDelivered)
        {
            // Award loyalty points upon actual delivery
            var earnedPoints = (int)((order.TotalAmount ?? 0m) / LoyaltyConstants.EarnRateAmount);
            if (earnedPoints > 0)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId)
                    ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

                customer.Points += earnedPoints;
                _unitOfWork.Customers.Update(customer);
            }

            order.PointsEarned = earnedPoints;

            // Automatically complete pending Cash on Delivery payments upon delivery confirmation
            var payment = await _unitOfWork.Payments.GetByOrderIdAsync(order.OrderId);
            if (payment is not null &&
                payment.PaymentMethod == PaymentMethods.Cash &&
                payment.Status == PaymentStatuses.Pending)
            {
                payment.Status = PaymentStatuses.Completed;
                _unitOfWork.Payments.Update(payment);
            }
        }

        if (newStatusId == StatusCancelled)
        {
            // Increment stock atomically directly in database upon cancellation
            foreach (var item in order.OrderItems)
            {
                await _unitOfWork.Products.IncrementStockAsync(item.ProductId, item.Quantity);
            }

            if (order.PointsRedeemed > 0)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId)
                    ?? throw new NotFoundException(nameof(Customer), order.CustomerId);

                customer.Points += order.PointsRedeemed;
                _unitOfWork.Customers.Update(customer);
            }
        }

        order.OrderStatusId = newStatusId;
        _unitOfWork.Orders.Update(order);
    }

    private static async Task<PagedResult<OrderSummaryReadDto>> BuildPagedSummaryAsync(
        IQueryable<Order> query, OrderQueryParams queryParams)
    {
        if (queryParams.OrderStatusId.HasValue)
        {
            query = query.Where(o => o.OrderStatusId == queryParams.OrderStatusId.Value);
        }

        query = queryParams.SortDescending
            ? query.OrderByDescending(o => o.OrderDate)
            : query.OrderBy(o => o.OrderDate);

        var projectedQuery = query.Select(o => new OrderSummaryReadDto
        {
            OrderId = o.OrderId,
            OrderDate = o.OrderDate,
            OrderStatusName = o.OrderStatus!.Name,
            TotalAmount = o.TotalAmount ?? 0m,
            ItemsCount = o.OrderItems.Count
        });

        return await PagedResult<OrderSummaryReadDto>.CreateAsync(
            projectedQuery, queryParams.PageNumber, queryParams.PageSize);
    }

    private static OrderReadDto MapToReadDto(Order order)
    {
        return new OrderReadDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            OrderStatusId = order.OrderStatusId ?? InitialOrderStatusId,
            OrderStatusName = order.OrderStatus?.Name ?? string.Empty,
            Comments = order.Comments,
            ShippedDate = order.ShippedDate,
            ShipperId = order.ShipperId,
            ShipperName = order.Shipper?.Name,
            TotalAmount = order.TotalAmount ?? 0m,
            PointsRedeemed = order.PointsRedeemed,
            PointsEarned = order.PointsEarned,
            Items = order.OrderItems.Select(oi => new OrderItemReadDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }
}