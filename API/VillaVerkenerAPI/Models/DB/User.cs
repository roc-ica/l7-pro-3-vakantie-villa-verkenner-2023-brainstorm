namespace VillaVerkenerAPI.Models.DB;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public sbyte IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
