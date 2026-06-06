using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Component
{
    public int Id { get; set; }

    public string? Category { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public bool? InStook { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Keyboardconfiguration> KeyboardconfigurationCasesizes { get; set; } = new List<Keyboardconfiguration>();

    public virtual ICollection<Keyboardconfiguration> KeyboardconfigurationKeycaps { get; set; } = new List<Keyboardconfiguration>();

    public virtual ICollection<Keyboardconfiguration> KeyboardconfigurationSwitchtypes { get; set; } = new List<Keyboardconfiguration>();
}
