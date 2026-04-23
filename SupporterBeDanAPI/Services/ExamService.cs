using Microsoft.EntityFrameworkCore;
using SupporterBeDanAPI.DTOs.Exam;
using SupporterBeDanAPI.Helpers;
using SupporterBeDanAPI.Models;

namespace SupporterBeDanAPI.Services;

public interface IExamService
{
    Task<List<ExamResponse>> GetAllExamsAsync();
    Task<List<ExamResponse>> GetAllExamsByDateAsync(DateOnly date);
    Task<List<ExamResponse>> GetExamsBySupporterAsync(int supporterId);
    Task<List<DateOnly>> GetAllExamDatesAsync();
    Task<ExamResponse?> GetExamByIdAsync(int id);
    Task<ExamResponse?> CreateExamAsync(CreateExamRequest request, int userId);
    Task<ExamResponse?> UpdateExamAsync(int id, UpdateExamRequest request, int userId);
    Task<bool> DeleteExamAsync(int id, int userId);
    Task<List<ExamHistoryResponse>> GetUserExamHistoryAsync(int userId);
    Task<List<ExamResponse>> GetPendingExamsAsync();
    Task<ExamResponse?> ApproveExamAsync(int id, ApproveExamRequest request, int approverId);
    Task<ExamResponse?> CompleteExamAsync(int id, CompleteExamRequest request, int updaterId);
    Task<List<StatusOption>> GetRegistrationStatusesAsync();
    Task<List<StatusOption>> GetCompletionStatusesAsync();
}

public class ExamService : IExamService
{
    private readonly SupporterBeDanContext _context;
    private readonly ILogger<ExamService> _logger;

    public ExamService(SupporterBeDanContext context, ILogger<ExamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ExamResponse>> GetAllExamsAsync()
    {
        return await _context.ExamRegistrations
            .Include(e => e.User)
                .ThenInclude(u => u.Role)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapToExamResponse(e))
            .ToListAsync();
    }

    public async Task<List<ExamResponse>> GetAllExamsByDateAsync(DateOnly date)
    {
        return await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .Where(e => e.ExamDate == date)
            .OrderBy(e => e.Slot)
            .Select(e => MapToExamResponse(e))
            .ToListAsync();
    }

    public async Task<List<ExamResponse>> GetExamsBySupporterAsync(int supporterId)
    {
        return await _context.ExamRegistrations
            .Include(e => e.User)
                .ThenInclude(u => u.Role)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .Where(e => e.ExamAssignments.Any(a => a.SupporterId == supporterId))
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapToExamResponse(e))
            .ToListAsync();
    }

    public async Task<List<DateOnly>> GetAllExamDatesAsync()
    {
        return await _context.ExamRegistrations
            .Select(e => e.ExamDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();
    }

    public async Task<ExamResponse?> GetExamByIdAsync(int id)
    {
        var exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstOrDefaultAsync(e => e.Id == id);

        return exam == null ? null : MapToExamResponse(exam);
    }

    public async Task<ExamResponse?> CreateExamAsync(CreateExamRequest request, int userId)
    {
        if (request.SupporterId <= 0)
        {
            _logger.LogWarning("Create exam failed: Invalid SupporterId {SupporterId}", request.SupporterId);
            return null;
        }

        var supporterExists = await _context.Users.AnyAsync(u => u.Id == request.SupporterId);
        if (!supporterExists)
        {
            _logger.LogWarning("Create exam failed: Supporter {SupporterId} not found", request.SupporterId);
            return null;
        }

        var exam = new ExamRegistration
        {
            UserId = userId,
            Subject = request.Subject,
            ExamDate = request.ExamDate,
            Slot = request.Slot,
            Spcode = request.Spcode,
            PaymentStatus = request.PaymentStatus ?? "Unpaid",
            ContactInfo = request.ContactInfo,
            RegistrationStatusId = 1,
            ExamCompletionStatusId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.ExamRegistrations.Add(exam);
        await _context.SaveChangesAsync();

        var assignment = new ExamAssignment
        {
            ExamRegistrationId = exam.Id,
            SupporterId = request.SupporterId,
            AssignedAt = DateTime.UtcNow
        };

        _context.ExamAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Exam created with ID {ExamId} for User {UserId} with Supporter {SupporterId}", exam.Id, exam.UserId, request.SupporterId);

        exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstAsync(e => e.Id == exam.Id);

        return MapToExamResponse(exam);
    }

    public async Task<ExamResponse?> UpdateExamAsync(int id, UpdateExamRequest request, int userId)
    {
        var exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
        {
            _logger.LogWarning("Update exam failed: Exam {ExamId} not found", id);
            return null;
        }

        var currentSupporterId = exam.ExamAssignments.FirstOrDefault()?.SupporterId ?? 0;
        if (exam.UserId != userId && currentSupporterId != userId)
        {
            _logger.LogWarning("Update exam failed: User {UserId} not authorized to update exam {ExamId}", userId, id);
            return null;
        }

        if (request.SupporterId.HasValue && request.SupporterId.Value != currentSupporterId)
        {
            var supporterExists = await _context.Users.AnyAsync(u => u.Id == request.SupporterId.Value);
            if (!supporterExists)
            {
                _logger.LogWarning("Update exam failed: New Supporter {SupporterId} not found", request.SupporterId.Value);
                return null;
            }

            var existingAssignment = exam.ExamAssignments.FirstOrDefault();
            if (existingAssignment != null)
            {
                existingAssignment.SupporterId = request.SupporterId.Value;
                existingAssignment.AssignedAt = DateTime.UtcNow;
            }
            else
            {
                exam.ExamAssignments.Add(new ExamAssignment
                {
                    ExamRegistrationId = exam.Id,
                    SupporterId = request.SupporterId.Value,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        if (!string.IsNullOrEmpty(request.Subject))
            exam.Subject = request.Subject;

        if (request.ExamDate.HasValue)
            exam.ExamDate = request.ExamDate.Value;

        if (!string.IsNullOrEmpty(request.Slot))
            exam.Slot = request.Slot;

        if (request.Spcode != null)
            exam.Spcode = request.Spcode;

        if (!string.IsNullOrEmpty(request.PaymentStatus))
            exam.PaymentStatus = request.PaymentStatus;

        if (request.ContactInfo != null)
            exam.ContactInfo = request.ContactInfo;

        if (request.RegistrationStatusId.HasValue)
            exam.RegistrationStatusId = request.RegistrationStatusId.Value;

        if (request.ExamCompletionStatusId.HasValue)
            exam.ExamCompletionStatusId = request.ExamCompletionStatusId.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} updated by user {UserId}", id, userId);

        exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstAsync(e => e.Id == id);

        return MapToExamResponse(exam);
    }

    public async Task<bool> DeleteExamAsync(int id, int userId)
    {
        var exam = await _context.ExamRegistrations
            .Include(e => e.ExamAssignments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
        {
            _logger.LogWarning("Delete exam failed: Exam {ExamId} not found", id);
            return false;
        }

        var currentSupporterId = exam.ExamAssignments.FirstOrDefault()?.SupporterId ?? 0;
        if (exam.UserId != userId && currentSupporterId != userId)
        {
            _logger.LogWarning("Delete exam failed: User {UserId} not authorized to delete exam {ExamId}", userId, id);
            return false;
        }

        _context.ExamRegistrations.Remove(exam);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} deleted by user {UserId}", id, userId);
        return true;
    }

    public async Task<List<ExamHistoryResponse>> GetUserExamHistoryAsync(int userId)
    {
        return await _context.ExamRegistrations
            .Include(e => e.User)
                .ThenInclude(u => u.Role)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.ExamDate)
            .Select(e => new ExamHistoryResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                UserFullName = e.User.FullName,
                UserRoleName = e.User.Role.Name,
                SupporterId = e.ExamAssignments.FirstOrDefault() != null ? e.ExamAssignments.FirstOrDefault().SupporterId : 0,
                SupporterFullName = e.ExamAssignments.FirstOrDefault() != null ? e.ExamAssignments.FirstOrDefault().Supporter.FullName : null,
                SupporterRoleName = e.ExamAssignments.FirstOrDefault() != null ? e.ExamAssignments.FirstOrDefault().Supporter.Role.Name : null,
                Subject = e.Subject,
                ExamDate = e.ExamDate,
                Slot = e.Slot,
                Spcode = e.Spcode,
                PaymentStatus = e.PaymentStatus,
                ContactInfo = e.ContactInfo,
                CreatedAt = e.CreatedAt,
                RegistrationStatusId = e.RegistrationStatusId,
                RegistrationStatusName = e.RegistrationStatus.Name,
                ExamCompletionStatusId = e.ExamCompletionStatusId,
                ExamCompletionStatusName = e.ExamCompletionStatus.Name
            })
            .ToListAsync();
    }

    public async Task<List<ExamResponse>> GetPendingExamsAsync()
    {
        return await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .Where(e => e.RegistrationStatusId == 1)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => MapToExamResponse(e))
            .ToListAsync();
    }

    public async Task<ExamResponse?> ApproveExamAsync(int id, ApproveExamRequest request, int approverId)
    {
        var exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
        {
            _logger.LogWarning("Approve exam failed: Exam {ExamId} not found", id);
            return null;
        }

        var statusExists = await _context.RegistrationStatuses.AnyAsync(s => s.Id == request.RegistrationStatusId);
        if (!statusExists)
        {
            _logger.LogWarning("Approve exam failed: RegistrationStatus {StatusId} not found", request.RegistrationStatusId);
            return null;
        }

        exam.RegistrationStatusId = request.RegistrationStatusId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} approved/rejected by user {ApproverId}", id, approverId);

        exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstAsync(e => e.Id == id);

        return MapToExamResponse(exam);
    }

    public async Task<ExamResponse?> CompleteExamAsync(int id, CompleteExamRequest request, int updaterId)
    {
        var exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
        {
            _logger.LogWarning("Complete exam failed: Exam {ExamId} not found", id);
            return null;
        }

        var statusExists = await _context.ExamCompletionStatuses.AnyAsync(s => s.Id == request.ExamCompletionStatusId);
        if (!statusExists)
        {
            _logger.LogWarning("Complete exam failed: ExamCompletionStatus {StatusId} not found", request.ExamCompletionStatusId);
            return null;
        }

        exam.ExamCompletionStatusId = request.ExamCompletionStatusId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Exam {ExamId} completion status updated by user {UpdaterId}", id, updaterId);

        exam = await _context.ExamRegistrations
            .Include(e => e.User)
            .Include(e => e.ExamAssignments)
                .ThenInclude(a => a.Supporter)
                    .ThenInclude(s => s.Role)
            .Include(e => e.RegistrationStatus)
            .Include(e => e.ExamCompletionStatus)
            .FirstAsync(e => e.Id == id);

        return MapToExamResponse(exam);
    }

    public async Task<List<StatusOption>> GetRegistrationStatusesAsync()
    {
        return await _context.RegistrationStatuses
            .Select(s => new StatusOption { Id = s.Id, Name = s.Name })
            .ToListAsync();
    }

    public async Task<List<StatusOption>> GetCompletionStatusesAsync()
    {
        return await _context.ExamCompletionStatuses
            .Select(s => new StatusOption { Id = s.Id, Name = s.Name })
            .ToListAsync();
    }

    private static ExamResponse MapToExamResponse(ExamRegistration exam)
    {
        var assignment = exam.ExamAssignments?.FirstOrDefault();
        return new ExamResponse
        {
            Id = exam.Id,
            UserId = exam.UserId,
            UserFullName = exam.User?.FullName,
            UserRoleName = exam.User?.Role?.Name,
            UserFacebook = exam.User?.Facebook,
            UserPhone = exam.User?.Phone,
            SupporterId = assignment?.SupporterId ?? 0,
            SupporterFullName = assignment?.Supporter?.FullName,
            SupporterRoleName = assignment?.Supporter?.Role?.Name,
            Subject = exam.Subject,
            ExamDate = exam.ExamDate,
            Slot = exam.Slot,
            Spcode = exam.Spcode,
            PaymentStatus = exam.PaymentStatus,
            ContactInfo = exam.ContactInfo,
            CreatedAt = exam.CreatedAt,
            RegistrationStatusId = exam.RegistrationStatusId,
            RegistrationStatusName = exam.RegistrationStatus?.Name ?? string.Empty,
            ExamCompletionStatusId = exam.ExamCompletionStatusId,
            ExamCompletionStatusName = exam.ExamCompletionStatus?.Name ?? string.Empty
        };
    }
}
