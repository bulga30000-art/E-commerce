using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_commerce.DTOs.Auth;
using E_commerce.Exceptions;
using E_commerce.Identity;
using E_commerce.Models;
using E_commerce.Repositories.Interfaces;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace E_commerce.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    // Default role assigned to self-registered users
    private const string DefaultRole = "Customer";

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1) Verify email uniqueness
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            throw new ConflictException("هذا البريد الإلكتروني مسجل بالفعل.");
        }

        // 2) Initialize ApplicationUser entity
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.Phone
        };

        // Wrap registration in a database transaction to guarantee atomicity across Identity and Domain Customer records
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"فشل إنشاء الحساب: {errors}");
            }

            // 3) Assign default Customer role
            var roleResult = await _userManager.AddToRoleAsync(user, DefaultRole);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(" | ", roleResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"فشل تعيين الصلاحية: {errors}");
            }

            // 4) Create linked Customer entity
            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                Points = 0,
                UserId = user.Id
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            // 5) Build and return auth token response
            return await BuildAuthResponseAsync(user, customer, DefaultRole);
        }
        catch
        {
            // Roll back transaction on error to prevent dangling Identity records
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Standardized security error message for both non-existent user and invalid password
        if (user is null)
        {
            throw new BadRequestException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            throw new BadRequestException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");
        }

        var customer = await _unitOfWork.Customers.GetByUserIdAsync(user.Id);
        if (customer is null)
        {
            throw new BadRequestException("لا يوجد حساب عميل مرتبط بهذا المستخدم.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? DefaultRole;

        return await BuildAuthResponseAsync(user, customer, role);
    }

    public async Task PromoteToAdminAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            throw new NotFoundException($"لا يوجد مستخدم بالبريد الإلكتروني '{email}'.");
        }

        // Verify user is not already an Admin
        var isAlreadyAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAlreadyAdmin)
        {
            throw new ConflictException($"المستخدم '{email}' لديه صلاحية Admin بالفعل.");
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"فشل تعيين صلاحية Admin: {errors}");
        }
    }

    // Helper method to construct JWT token payload and response DTO
    private Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, Customer customer, string role)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        // Encode user identity details and role into JWT claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("customerId", customer.CustomerId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new AuthResponseDto
        {
            Token = tokenString,
            ExpiresAtUtc = expiresAt,
            Email = user.Email!,
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Role = role
        };

        return Task.FromResult(response);
    }
}