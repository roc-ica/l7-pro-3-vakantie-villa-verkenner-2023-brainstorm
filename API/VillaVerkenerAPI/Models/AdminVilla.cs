using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

namespace VillaVerkenerAPI.Models
{
    public class AdminVilla
    {

        public int VillaID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public string VillaImagePath { get; set; }

        public List<AdminRequest> Requests { get; set; } = new List<AdminRequest>();
        public AdminVilla(Villa villa, List<AdminRequest> requests)
        {
            VillaID = villa.VillaId;
            Name = villa.Naam;
            Price = villa.Prijs;
            Location = villa.Locatie;
            VillaImagePath = villa.Images.Count > 0 ? villa.Images.Where(image => image.IsPrimary == 1).First().ImageLocation : "";
            VillaImagePath = APIUrlHandler.GetImageUrl(VillaImagePath);
            Requests = requests;
        }

        public static AdminVilla From(Villa villa, List<AdminRequest> requests)
        {
            return new AdminVilla(villa, requests);
        }

    }
}
