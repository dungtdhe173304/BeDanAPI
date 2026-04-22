using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupporterBeDanAPI.DTOs.Exam;
using SupporterBeDanAPI.Helpers;
using SupporterBeDanAPI.Services;

namespace SupporterBeDanAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(IExamService examService, ILogger<ExamsController> logger)
    {
        _examService = examService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllExams()
    {
        var exams = await _examService.GetAllExamsAsync();
        return Ok(ApiResponse.Success(exams, "Lấy danh sách lịch thi thành công"));
    }

    [HttpGet("dates")]
    public async Task<IActionResult> GetAllExamDates()
    {
        var dates = await _examService.GetAllExamDatesAsync();
        return Ok(ApiResponse.Success(dates, "Lấy danh sách ngày thi thành công"));
    }

    [HttpGet("by-date/{date}")]
    public async Task<IActionResult> GetExamsByDate(DateOnly date)
    {
        var exams = await _examService.GetAllExamsByDateAsync(date);
        return Ok(ApiResponse.Success(exams, $"Lấy danh sách lịch thi ngày {date:yyyy-MM-dd} thành công"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExamById(int id)
    {
        var exam = await _examService.GetExamByIdAsync(id);
        if (exam == null)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy lịch thi"));
        }
        return Ok(ApiResponse.Success(exam, "Lấy thông tin lịch thi thành công"));
    }

    [HttpPost]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Slot))
        {
            return BadRequest(ApiResponse.BadRequest("Subject và Slot không được để trống"));
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var exam = await _examService.CreateExamAsync(request, userId);
        if (exam == null)
        {
            return BadRequest(ApiResponse.Error("Tạo lịch thi thất bại. Vui lòng kiểm tra lại UserId."));
        }

        return CreatedAtAction(nameof(GetExamById), new { id = exam.Id }, ApiResponse.Success(exam, "Tạo lịch thi thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExam(int id, [FromBody] UpdateExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var exam = await _examService.UpdateExamAsync(id, request, userId);
        if (exam == null)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy lịch thi hoặc bạn không có quyền sửa"));
        }

        return Ok(ApiResponse.Success(exam, "Cập nhật lịch thi thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExam(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var result = await _examService.DeleteExamAsync(id, userId);
        if (!result)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy lịch thi hoặc bạn không có quyền xóa"));
        }

        return Ok(ApiResponse.Success(null, "Xóa lịch thi thành công"));
    }

    [HttpGet("history/me")]
    public async Task<IActionResult> GetMyExamHistory()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var history = await _examService.GetUserExamHistoryAsync(userId);
        return Ok(ApiResponse.Success(history, "Lấy lịch sử đăng ký lịch thi thành công"));
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetUserExamHistory(int userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        if (currentUserId != userId)
        {
            return Forbid();
        }

        var history = await _examService.GetUserExamHistoryAsync(userId);
        return Ok(ApiResponse.Success(history, "Lấy lịch sử đăng ký lịch thi thành công"));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Supporter")]
    public async Task<IActionResult> GetPendingExams()
    {
        var exams = await _examService.GetPendingExamsAsync();
        return Ok(ApiResponse.Success(exams, "Lấy danh sách lịch thi chờ duyệt thành công"));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Supporter")]
    public async Task<IActionResult> ApproveExam(int id, [FromBody] ApproveExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var exam = await _examService.ApproveExamAsync(id, request, userId);
        if (exam == null)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy lịch thi hoặc trạng thái không hợp lệ"));
        }

        return Ok(ApiResponse.Success(exam, "Cập nhật trạng thái duyệt thành công"));
    }

    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Admin,Supporter")]
    public async Task<IActionResult> CompleteExam(int id, [FromBody] CompleteExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(ApiResponse.Unauthorized("Không xác định được người dùng"));
        }

        var exam = await _examService.CompleteExamAsync(id, request, userId);
        if (exam == null)
        {
            return NotFound(ApiResponse.NotFound("Không tìm thấy lịch thi hoặc trạng thái không hợp lệ"));
        }

        return Ok(ApiResponse.Success(exam, "Cập nhật trạng thái hoàn thành ca thi thành công"));
    }

    [HttpGet("statuses/registration")]
    public async Task<IActionResult> GetRegistrationStatuses()
    {
        var statuses = await _examService.GetRegistrationStatusesAsync();
        return Ok(ApiResponse.Success(statuses, "Lấy danh sách trạng thái đăng ký thành công"));
    }

    [HttpGet("statuses/completion")]
    public async Task<IActionResult> GetCompletionStatuses()
    {
        var statuses = await _examService.GetCompletionStatusesAsync();
        return Ok(ApiResponse.Success(statuses, "Lấy danh sách trạng thái hoàn thành thành công"));
    }
}
