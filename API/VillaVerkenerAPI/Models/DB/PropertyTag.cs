namespace VillaVerkenerAPI.Models.DB;

public partial class PropertyTag
{
    public int PropertyTagId { get; set; }

    public string PropertyTag1 { get; set; } = null!;

    public virtual ICollection<VillaPropertyTag> VillaPropertyTags { get; set; } = new List<VillaPropertyTag>();
}
