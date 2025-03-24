using VillaVerkenerAPI.Models.DB;
using static System.Net.WebRequestMethods;

namespace VillaVerkenerAPI.Models
{
    public class SmallVilla
    {
        public int VillaID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string VillaImagePath { get; set; }

        public SmallVilla(int villaID, string name, decimal price, string location, int capacity, string villaImagePath, int bedrooms, int bathrooms)
        {
            VillaID = villaID;
            Name = name;
            Price = price;
            Location = location;
            Capacity = capacity;
            Bedrooms = bedrooms;
            Bathrooms = bathrooms;
            VillaImagePath = "http://localhost:3012/Images/" + villaImagePath;
        }
        public SmallVilla(Villa villa)
        {
            VillaID = villa.VillaId;
            Name = villa.Naam;
            Price = villa.Prijs;
            Location = villa.Locatie;
            Capacity = villa.Capaciteit;
            Bedrooms = villa.Slaapkamers;
            Bathrooms = villa.Badkamers;
            VillaImagePath = villa.Images.Count > 0 ? villa.Images.Where(image => image.IsPrimary == 1).First().ImageLocation : "";
            VillaImagePath = "http://localhost:3012/Images/" + VillaImagePath;
        }

        public static SmallVilla From(Villa villa)
        {
            return new SmallVilla(villa);
        }
    }
}
