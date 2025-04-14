namespace VillaVerkenerAPI.Models.DB;

public partial class VillaLocationTag
{
    public int Id { get; set; }

    public int VillaId { get; set; }

    public int LocationTagId { get; set; }

    public virtual LocationTag LocationTag { get; set; } = null!;

    public virtual Villa Villa { get; set; } = null!;
}
