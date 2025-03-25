using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models.DB;

public partial class Session
{
    public int SessionId { get; set; }

    public string SessionKey { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime ExpirationDate { get; set; }

    public virtual User User { get; set; } = null!;
}
