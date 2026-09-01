// Repositories/Interfaces/IPaymentRepository.cs
using E_commerce.Models;

namespace E_commerce.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);

    // Retrieves payment record associated with a specific order ID.
    Task<Payment?> GetByOrderIdAsync(int orderId);

    Task AddAsync(Payment payment);
    void Update(Payment payment);
    void Delete(Payment payment);
}