using E_commerce.DTOs.Auth;

namespace E_commerce.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    // Promotes an existing user to Admin role. Protected by [Authorize(Roles = "Admin")] in Controller.
    Task PromoteToAdminAsync(string email);
}