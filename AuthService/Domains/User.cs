using System;
using System.Collections.Generic;

namespace AuthService.Domains;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int AccessLevelId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AccessLevel AccessLevel { get; set; } = null!;
}
