namespace SupporterBeDanAPI.Models;

public partial class ExamRegistration
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Subject { get; set; } = null!;

    public DateOnly ExamDate { get; set; }

    public string Slot { get; set; } = null!;

    public string? Spcode { get; set; }

    public string? PaymentStatus { get; set; }

    public string? ContactInfo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RegistrationStatusId { get; set; }

    public int ExamCompletionStatusId { get; set; }

    public virtual ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();

    public virtual User User { get; set; } = null!;

    public virtual RegistrationStatus RegistrationStatus { get; set; } = null!;

    public virtual ExamCompletionStatus ExamCompletionStatus { get; set; } = null!;
}
