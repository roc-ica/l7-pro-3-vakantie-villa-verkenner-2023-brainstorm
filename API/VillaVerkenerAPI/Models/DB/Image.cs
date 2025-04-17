namespace VillaVerkenerAPI.Models.DB;

public partial class Image
{
    public int VillaImageId { get; set; }

    public int VillaId { get; set; }

    public string ImageLocation { get; set; } = null!;

    public sbyte IsPrimary { get; set; }

    public virtual Villa Villa { get; set; } = null!;
}
