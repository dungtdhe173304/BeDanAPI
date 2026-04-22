using System.Collections.Generic;

namespace SupporterBeDanAPI.Models;

public partial class RegistrationStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = new List<ExamRegistration>();
}
