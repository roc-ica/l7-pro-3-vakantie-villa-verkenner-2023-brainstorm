using System;
using System.Collections.Generic;

namespace VillaVerkenerAPI.Models.DB;

public partial class Request
{
    public int RequestId { get; set; }

    public int VillaId { get; set; }

    public string Email { get; set; } = null!;

    public sbyte IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string Message { get; set; } = null!;

    public virtual Villa Villa { get; set; } = null!;
}
