namespace E_commerce.DTOs.Auth;

// Response DTO returned upon successful registration or authentication
public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public string Email { get; set; } = null!;
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
}