using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models
{
    public class EditVilla : DetailedVilla
    {
        public List<int> PropertyTags { get; set; } = new();
        public List<int> LocationTags { get; set; } = new();
        public List<string> PropertyNames { get; set; }
        public List<string> LocationNames { get; set; }

        public EditVilla(Villa villa, DBContext _dbContext)
            : base(villa)
        {
            PropertyTags = villa.VillaPropertyTags
                .Select(vpt => _dbContext.PropertyTags.FirstOrDefault(pt => pt.PropertyTagId == vpt.PropertyTagId)?.PropertyTagId)
                .Where(pt => pt.HasValue) // Filter out null values
                .Select(pt => pt.Value) // Convert nullable int to int
                .ToList();

            LocationTags = villa.VillaLocationTags
                .Select(vlt => _dbContext.LocationTags.FirstOrDefault(lt => lt.LocationTagId == vlt.LocationTagId)?.LocationTagId)
                .Where(lt => lt.HasValue) // Filter out null values
                .Select(lt => lt.Value) // Convert nullable int to int
                .ToList();

            PropertyNames = _dbContext.PropertyTags
                .Where(dpt => PropertyTags.Contains(dpt.PropertyTagId))
                .Select(t => t.PropertyTag1)
                .ToList();

            LocationNames = _dbContext.LocationTags
                .Where(dpt => LocationTags.Contains(dpt.LocationTagId))
                .Select(t => t.LocationTag1)
                .ToList();
        }

        public static new EditVilla From(Villa villa, DBContext _dbContext)
        {
            return new EditVilla(villa, _dbContext);
        }
    }
}
