using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models
{
    public class DetailedVilla
    {
        public int VillaID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; }
        public string VillaMainImagePath { get; set; }
        public List<string> VillaImagePaths { get; set; }

        public DetailedVilla(int villaID, string name,string description, decimal price, string location, int capacity, string villaImagePath, int bedrooms, int bathrooms, List<string> imagePaths)
        {
            VillaID = villaID;
            Name = name;
            Price = price;
            Location = location;
            Capacity = capacity;
            Bedrooms = bedrooms;
            Bathrooms = bathrooms;
            VillaMainImagePath = villaImagePath;
            VillaImagePaths = imagePaths;
            Description = description;
        }
        public DetailedVilla(Villa villa)
        {
            VillaID = villa.VillaId;
            Name = villa.Naam;
            Price = villa.Prijs;
            Location = villa.Locatie;
            Capacity = villa.Capaciteit;
            Bedrooms = villa.Slaapkamers;
            Bathrooms = villa.Badkamers;
            VillaMainImagePath = villa.Images.Count > 0 ? villa.Images.Where(image => image.IsPrimary == 1).First().ImageLocation : "";
            VillaImagePaths = villa.Images.Count > 0 ? villa.Images.Where(image => image.IsPrimary == 0).ToList().Select(image => image.ImageLocation).ToList() : new();
            VillaMainImagePath = "http://localhost:3012/Images/" + VillaMainImagePath;
            VillaImagePaths = VillaImagePaths.Select(imagePath => "http://localhost:3012/Images/" + imagePath).ToList();
            Description = villa.Omschrijving;
        }

        public static DetailedVilla From(Villa villa)
        {
            return new DetailedVilla(villa);
        }
    }
}
