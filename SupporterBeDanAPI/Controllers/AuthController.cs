using Microsoft.AspNetCore.Mvc;
using SupporterBeDanAPI.DTOs.Auth;
using SupporterBeDanAPI.Helpers;
using SupporterBeDanAPI.Services;

namespace SupporterBeDanAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.BadRequest("Username và Password không được để trống"));
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(ApiResponse.BadRequest("Password phải có ít nhất 6 ký tự"));
        }

        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return Conflict(ApiResponse.Error("Username đã tồn tại"));
        }

        return Ok(ApiResponse.Success(result, "Đăng ký thành công"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.BadRequest("Username và Password không được để trống"));
        }

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(ApiResponse.Unauthorized("Username hoặc Password không đúng"));
        }

        return Ok(ApiResponse.Success(result, "Đăng nhập thành công"));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Unauthorized("Token không hợp lệ"));
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy người dùng"));
        }

        return Ok(ApiResponse.Success(user, "Lấy thông tin người dùng thành công"));
    }
}
