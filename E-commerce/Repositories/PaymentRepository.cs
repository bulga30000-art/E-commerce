// Repositories/PaymentRepository.cs
using E_commerce.Data;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly StoreContext _context;

    public PaymentRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public void Update(Payment payment)
    {
        _context.Payments.Update(payment);
    }

    public void Delete(Payment payment)
    {
        _context.Payments.Remove(payment);
    }
}