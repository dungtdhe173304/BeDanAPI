namespace SupporterBeDanAPI.Models;

public class ExamAssignment
{
    public int Id { get; set; }
    
    public int ExamRegistrationId { get; set; }
    
    public int SupporterId { get; set; }
    
    public DateTime? AssignedAt { get; set; }
    
    public virtual ExamRegistration ExamRegistration { get; set; } = null!;
    
    public virtual User Supporter { get; set; } = null!;
}
