using E_commerce.DTOs.Common;
using E_commerce.DTOs.Customer;

namespace E_commerce.Services.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerReadDto>> GetPagedAsync(CustomerQueryParams queryParams);

    Task<CustomerReadDto> GetByIdAsync(int id);

    Task<CustomerReadDto> UpdateAsync(int id, CustomerUpdateDto dto);

    Task DeleteAsync(int id);
}