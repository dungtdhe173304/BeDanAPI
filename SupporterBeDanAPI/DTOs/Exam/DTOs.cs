namespace SupporterBeDanAPI.DTOs.Exam;

public class CreateExamRequest
{
    public int SupporterId { get; set; }
    public string Subject { get; set; } = null!;
    public DateOnly ExamDate { get; set; }
    public string Slot { get; set; } = null!;
    public string? Spcode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ContactInfo { get; set; }
}

public class UpdateExamRequest
{
    public int? SupporterId { get; set; }
    public string? Subject { get; set; }
    public DateOnly? ExamDate { get; set; }
    public string? Slot { get; set; }
    public string? Spcode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ContactInfo { get; set; }
    public int? RegistrationStatusId { get; set; }
    public int? ExamCompletionStatusId { get; set; }
}

public class ApproveExamRequest
{
    public int RegistrationStatusId { get; set; }
}

public class CompleteExamRequest
{
    public int ExamCompletionStatusId { get; set; }
}

public class ExamResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserRoleName { get; set; }
    public string? UserFacebook { get; set; }
    public string? UserPhone { get; set; }
    public int SupporterId { get; set; }
    public string? SupporterFullName { get; set; }
    public string? SupporterRoleName { get; set; }
    public string Subject { get; set; } = null!;
    public DateOnly ExamDate { get; set; }
    public string Slot { get; set; } = null!;
    public string? Spcode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ContactInfo { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int RegistrationStatusId { get; set; }
    public string RegistrationStatusName { get; set; } = null!;
    public int ExamCompletionStatusId { get; set; }
    public string ExamCompletionStatusName { get; set; } = null!;
}

public class ExamHistoryResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserRoleName { get; set; }
    public int SupporterId { get; set; }
    public string? SupporterFullName { get; set; }
    public string? SupporterRoleName { get; set; }
    public string Subject { get; set; } = null!;
    public DateOnly ExamDate { get; set; }
    public string Slot { get; set; } = null!;
    public string? Spcode { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ContactInfo { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int RegistrationStatusId { get; set; }
    public string RegistrationStatusName { get; set; } = null!;
    public int ExamCompletionStatusId { get; set; }
    public string ExamCompletionStatusName { get; set; } = null!;
}

public class StatusOption
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
