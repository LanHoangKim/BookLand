using System;
using System.Collections.Generic;

namespace BookLand.Models;

public partial class AdminUser
{
    public int AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
