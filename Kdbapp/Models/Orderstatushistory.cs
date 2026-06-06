using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Orderstatushistory
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public int Status { get; set; }

    public long? Changedbyid { get; set; }

    public string? Comment { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
