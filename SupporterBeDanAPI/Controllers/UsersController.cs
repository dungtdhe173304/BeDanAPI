using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupporterBeDanAPI.Helpers;
using SupporterBeDanAPI.Models;

namespace SupporterBeDanAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly SupporterBeDanContext _context;

    public UsersController(SupporterBeDanContext context)
    {
        _context = context;
    }

    [HttpGet("supporters")]
    public async Task<IActionResult> GetSupporters()
    {
        var supporters = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role.Name == "Supporter")
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Username,
                RoleName = u.Role.Name
            })
            .ToListAsync();

        return Ok(ApiResponse.Success(supporters, "Lấy danh sách supporter thành công"));
    }
}
