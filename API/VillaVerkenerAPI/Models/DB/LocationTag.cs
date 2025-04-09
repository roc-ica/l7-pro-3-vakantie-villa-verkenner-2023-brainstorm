using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models.DB;

public partial class LocationTag
{
    public int LocationTagId { get; set; }

    public string LocationTag1 { get; set; } = null!;

    public virtual ICollection<VillaLocationTag> VillaLocationTags { get; set; } = new List<VillaLocationTag>();
}
