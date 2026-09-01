namespace E_commerce.Common;

// Order Status Identifiers matching seeded values in the database order_statuses table
// (IDENTITY starting from 1: Pending, Processing, Shipped, Delivered, Cancelled).
// Centralized here to provide a single source of truth across OrderService and PaymentService.
public static class OrderStatusIds
{
    public const byte Pending = 1;
    public const byte Processing = 2;
    public const byte Shipped = 3;
    public const byte Delivered = 4;
    public const byte Cancelled = 5;
}