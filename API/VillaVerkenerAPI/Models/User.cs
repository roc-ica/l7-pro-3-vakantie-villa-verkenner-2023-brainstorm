using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public sbyte IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
