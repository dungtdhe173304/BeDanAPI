using System;
using System.Collections.Generic;

namespace SupporterBeDanAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Facebook { get; set; }

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();

    public virtual Role Role { get; set; } = null!;
}
