using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models;

public partial class Image
{
    public int VillaImageId { get; set; }

    public int VillaId { get; set; }

    public string ImageLocation { get; set; } = null!;

    public virtual Villa Villa { get; set; } = null!;

    public virtual Villa? VillaNavigation { get; set; }
}
