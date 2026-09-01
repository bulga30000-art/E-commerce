using E_commerce.DTOs.Common;
using E_commerce.DTOs.Customer;
using E_commerce.Exceptions;
using E_commerce.Identity;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Services;

public class CustomerService : ICustomerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CustomerReadDto>> GetPagedAsync(CustomerQueryParams queryParams)
    {
        var query = _unitOfWork.Customers.GetQueryable();

        // Search across FirstName, LastName, Phone, City, and Address fields
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            var term = queryParams.SearchTerm.ToLower();
            query = query.Where(c =>
                EF.Functions.Like((c.FirstName + " " + c.LastName).ToLower(), $"%{term}%") ||
                EF.Functions.Like(c.Phone.ToLower(), $"%{term}%") ||
                EF.Functions.Like(c.City.ToLower(), $"%{term}%") ||
                EF.Functions.Like(c.Address.ToLower(), $"%{term}%"));
        }

        // Default sorting by FirstName and LastName
        query = queryParams.SortDescending
            ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
            : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);

        var projectedQuery = query.Select(c => new CustomerReadDto
        {
            CustomerId = c.CustomerId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Phone = c.Phone,
            Address = c.Address,
            City = c.City,
            Points = c.Points,
            UserId = c.UserId
        });

        return await PagedResult<CustomerReadDto>.CreateAsync(
            projectedQuery,
            queryParams.PageNumber,
            queryParams.PageSize);
    }

    public async Task<CustomerReadDto> GetByIdAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Customer), id);

        return new CustomerReadDto
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Points = customer.Points,
            UserId = customer.UserId
        };
    }

    public async Task<CustomerReadDto> UpdateAsync(int id, CustomerUpdateDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Customer), id);

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.City = dto.City;

        // Synchronize updated phone number to linked Identity user if UserId exists
        if (!string.IsNullOrEmpty(customer.UserId))
        {
            var user = await _userManager.FindByIdAsync(customer.UserId);
            if (user is not null)
            {
                user.PhoneNumber = dto.Phone;
            }
        }

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return new CustomerReadDto
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Points = customer.Points,
            UserId = customer.UserId
        };
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Customer), id);

        var hasOrders = await _unitOfWork.Orders.GetQueryable()
            .AnyAsync(o => o.CustomerId == id);

        if (hasOrders)
        {
            throw new ConflictException("لا يمكن حذف هذا العميل لأنه مرتبط بطلبات موجودة بالفعل");
        }

        _unitOfWork.Customers.Delete(customer);

        if (!string.IsNullOrEmpty(customer.UserId))
        {
            var appUser = await _userManager.FindByIdAsync(customer.UserId);
            if (appUser != null)
            {
                var result = await _userManager.DeleteAsync(appUser);
                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                    throw new BadRequestException($"فشل حذف حساب المستخدم المرتبط: {errors}");
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}