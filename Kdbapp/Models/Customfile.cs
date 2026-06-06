using System;
using System.Collections.Generic;

namespace Kdbapp.Models;

public partial class Customfile
{
    public long Id { get; set; }

    public long? ConfigId { get; set; }

    public string? Filetype { get; set; }

    public string? FileUrl { get; set; }

    public bool? IsChecked { get; set; }

    public virtual Keyboardconfiguration? Config { get; set; }
}
