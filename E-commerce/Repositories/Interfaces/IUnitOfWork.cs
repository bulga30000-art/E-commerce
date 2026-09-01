// Repositories/Interfaces/IUnitOfWork.cs
namespace E_commerce.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IOrderStatusRepository OrderStatuses { get; }
    ICustomerRepository Customers { get; }
    IPaymentRepository Payments { get; }
    IShipperRepository Shippers { get; }

    Task<int> SaveChangesAsync();

    // Abstraction methods wrapping DB transaction control (BEGIN, COMMIT, ROLLBACK).
    // Decouples the Service layer from EF Core transaction specifics (IDbContextTransaction).
    // Used in multi-step operations that require atomic completion (e.g. checkout, order cancellation).
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}