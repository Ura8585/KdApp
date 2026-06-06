using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Keyboardconfiguration
{
    public long Id { get; set; }

    public int? CasesizeId { get; set; }

    public int? SwitchtypeId { get; set; }

    public bool? IsSoundInsulated { get; set; }

    public string? CustommodelUrl { get; set; }

    public string? CustomprintUrl { get; set; }

    public string? Configurationhash { get; set; }

    public int? KeycapsId { get; set; }

    public long? UserId { get; set; }

    public virtual Component? Casesize { get; set; }

    public virtual ICollection<Customfile> Customfiles { get; set; } = new List<Customfile>();

    public virtual ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();

    public virtual Component? Keycaps { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Component? Switchtype { get; set; }

    public virtual User? User { get; set; }
}
