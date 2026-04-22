using Microsoft.EntityFrameworkCore;
using SupporterBeDanAPI.DTOs.Auth;
using SupporterBeDanAPI.Helpers;
using SupporterBeDanAPI.Models;

namespace SupporterBeDanAPI.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetUserByIdAsync(int id);
}

public class AuthService : IAuthService
{
    private readonly SupporterBeDanContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(SupporterBeDanContext context, JwtHelper jwtHelper, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
            return null;
        }

        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            _logger.LogError("Role 'User' not found in database");
            return null;
        }

        var user = new User
        {
            Username = request.Username,
            Password = PasswordHelper.HashPassword(request.Password),
            FullName = request.FullName,
            Facebook = request.Facebook,
            Phone = request.Phone,
            RoleId = userRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Username} registered successfully", user.Username);

        user.Role = userRole;
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User {Username} not found", request.Username);
            return null;
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
            return null;
        }

        _logger.LogInformation("User {Username} logged in successfully", user.Username);
        return GenerateAuthResponse(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return MapToUserDto(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var (token, expiresAt) = _jwtHelper.GenerateToken(user.Id, user.Username, user.Role.Name);
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = MapToUserDto(user)
        };
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Facebook = user.Facebook,
            Phone = user.Phone,
            RoleName = user.Role?.Name ?? string.Empty,
            CreatedAt = user.CreatedAt
        };
    }
}
