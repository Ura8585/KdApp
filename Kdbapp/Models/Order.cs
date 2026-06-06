using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Order
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int? Status { get; set; }

    public decimal TotalPrice { get; set; }

    public string? TrackNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Contactemail { get; set; }

    public long ConfigurationId { get; set; }

    public int? Quantity { get; set; }

    public virtual Keyboardconfiguration Configuration { get; set; } = null!;

    public virtual ICollection<Orderstatushistory> Orderstatushistories { get; set; } = new List<Orderstatushistory>();

    public virtual OrderStatus? StatusNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
