using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kdbapp.Models;

public partial class Keyboardconfiguration
{
    public long Id { get; set; }
    public int? CasesizeId { get; set; }
    public int? SwitchtypeId { get; set; }
    public int? KeycapsId { get; set; }
    public long? UserId { get; set; }
    public bool? IsSoundInsulated { get; set; }
    public string? CustommodelUrl { get; set; }
    public string? CustomprintUrl { get; set; }
    public string? Configurationhash { get; set; }

    // Новые поля
    public string? Layout { get; set; }
    public string? CaseColor { get; set; }
    public string? KeycapColor { get; set; }
    public string? VolumeColor { get; set; }
    public string? SwitchColor { get; set; }
    public string? KeycapMaterialType { get; set; }
    public string? SwitchType { get; set; }
    public bool? RgbMode { get; set; }
    public bool? HasCustomPrint { get; set; }
    public string? CustomPrintImageUrl { get; set; }
    public string? TotalPriceRaw { get; set; }

    [JsonIgnore]
    public virtual Component? Casesize { get; set; }
    [JsonIgnore]
    public virtual Component? Keycaps { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<Customfile> Customfiles { get; set; } = new List<Customfile>();
    public virtual ICollection<Gallery> Galleries { get; set; } = new List<Gallery>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}