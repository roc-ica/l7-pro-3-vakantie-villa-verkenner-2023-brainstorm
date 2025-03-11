using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models
{
    public class SmallVilla
    {
        public int VillaID { get; set; }
        public string Naam { get; set; }
        public decimal Prijs { get; set; }
        public string Locatie { get; set; }
        public int Capaciteit { get; set; }
        public Image VillaImage { get; set; }

        public SmallVilla(int villaID, string naam, decimal prijs, string locatie, int capaciteit, Image villaImage)
        {
            VillaID = villaID;
            Naam = naam;
            Prijs = prijs;
            Locatie = locatie;
            Capaciteit = capaciteit;
            VillaImage = villaImage;
        }
        public SmallVilla(Villa villa)
        {
            VillaID = villa.VillaId;
            Naam = villa.Naam;
            Prijs = villa.Prijs;
            Locatie = villa.Locatie;
            Capaciteit = villa.Capaciteit;
            VillaImage = villa.VillaImage;
        }

        public static SmallVilla From(Villa villa)
        {
            return new SmallVilla(villa);
        }
    }
}
