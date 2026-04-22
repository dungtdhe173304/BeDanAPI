namespace SupporterBeDanAPI.DTOs.Auth;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Facebook { get; set; }
    public string? Phone { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Facebook { get; set; }
    public string? Phone { get; set; }
    public string RoleName { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
}
