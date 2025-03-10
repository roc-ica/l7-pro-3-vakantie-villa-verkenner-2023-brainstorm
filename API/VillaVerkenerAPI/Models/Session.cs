using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models;

public partial class Session
{
    public string Session1 { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime ExpirationDate { get; set; }

    public virtual User User { get; set; } = null!;
}
