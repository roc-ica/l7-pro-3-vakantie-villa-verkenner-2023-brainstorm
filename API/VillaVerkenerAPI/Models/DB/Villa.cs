﻿namespace VillaVerkenerAPI.Models.DB;

public partial class Villa
{
    public int VillaId { get; set; }

    public string Naam { get; set; } = null!;

    public string Omschrijving { get; set; } = null!;

    public decimal Prijs { get; set; }

    public string Locatie { get; set; } = null!;

    public int Capaciteit { get; set; }

    public int Slaapkamers { get; set; }

    public int Badkamers { get; set; }

    public sbyte Verkocht { get; set; }

    public sbyte IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Image> Images { get; set; } = [];

    public virtual ICollection<Request> Requests { get; set; } = [];

    public virtual ICollection<VillaLocationTag> VillaLocationTags { get; set; } = [];

    public virtual ICollection<VillaPropertyTag> VillaPropertyTags { get; set; } = [];
}
