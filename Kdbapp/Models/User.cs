using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class User
{
    public long Id { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public DateTime? RegisteredAt { get; set; }

    public string? Role { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();

    public virtual ICollection<Keyboardconfiguration> Keyboardconfigurations { get; set; } = new List<Keyboardconfiguration>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
