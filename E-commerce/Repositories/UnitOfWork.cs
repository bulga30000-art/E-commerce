// Repositories/UnitOfWork.cs
using E_commerce.Data;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace E_commerce.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreContext _context;

    // EF Core transaction handle, instantiated upon calling BeginTransactionAsync.
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IOrderRepository? _orders;
    private IOrderItemRepository? _orderItems;
    private IOrderStatusRepository? _orderStatuses;
    private ICustomerRepository? _customers;
    private IPaymentRepository? _payments;
    private IShipperRepository? _shippers;

    public UnitOfWork(StoreContext context)
    {
        _context = context;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(_context);
    public IOrderStatusRepository OrderStatuses => _orderStatuses ??= new OrderStatusRepository(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public IShipperRepository Shippers => _shippers ??= new ShipperRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
        {
            // Throw invalid operation exception if Commit is invoked without an active transaction
            throw new InvalidOperationException("لا توجد Transaction بدأت بالفعل ليتم تأكيدها.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        // Safe check for null transaction to avoid throwing secondary exceptions inside catch blocks
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}