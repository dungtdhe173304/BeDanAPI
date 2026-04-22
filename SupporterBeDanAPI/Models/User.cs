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

    public virtual ICollection<ExamRegistration> ExamRegistrationSupporters { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<ExamRegistration> ExamRegistrationUsers { get; set; } = new List<ExamRegistration>();

    public virtual Role Role { get; set; } = null!;
}
