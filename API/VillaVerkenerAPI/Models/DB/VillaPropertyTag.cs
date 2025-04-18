﻿namespace VillaVerkenerAPI.Models.DB;

public partial class VillaPropertyTag
{
    public int Id { get; set; }

    public int VillaId { get; set; }

    public int PropertyTagId { get; set; }

    public virtual PropertyTag PropertyTag { get; set; } = null!;

    public virtual Villa Villa { get; set; } = null!;
}
