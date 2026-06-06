using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Gallery
{
    public long Id { get; set; }

    public long ConfigId { get; set; }

    public long? Authorid { get; set; }

    public string Title { get; set; } = null!;

    public bool? IsModerated { get; set; }

    public bool? Likescount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Author { get; set; }

    public virtual Keyboardconfiguration Config { get; set; } = null!;
}
