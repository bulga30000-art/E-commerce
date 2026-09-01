using E_commerce.DTOs.Auth;
using E_commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    // PUT api/auth/promote-to-admin/{email}
    // Protected endpoint: Only authenticated users with Admin role can execute this action.
    // Non-admin users receive HTTP 403 Forbidden automatically via [Authorize(Roles = "Admin")].
    [Authorize(Roles = "Admin")]
    [HttpPut("promote-to-admin/{email}")]
    public async Task<IActionResult> PromoteToAdmin(string email)
    {
        await _authService.PromoteToAdminAsync(email);
        return NoContent();
    }
}