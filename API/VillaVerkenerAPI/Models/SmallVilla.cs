using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models
{
    public class SmallVilla
    {
        public int VillaID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public Image VillaImage { get; set; }

        public SmallVilla(int villaID, string name, decimal price, string location, int capacity, Image villaImage)
        {
            VillaID = villaID;
            Name = name;
            Price = price;
            Location = location;
            Capacity = capacity;
            VillaImage = villaImage;
        }
        public SmallVilla(Villa villa)
        {
            VillaID = villa.VillaId;
            Name = villa.Naam;
            Price = villa.Prijs;
            Location = villa.Locatie;
            Capacity = villa.Capaciteit;
            VillaImage = villa.VillaImage;
        }

        public static SmallVilla From(Villa villa)
        {
            return new SmallVilla(villa);
        }
    }
}
