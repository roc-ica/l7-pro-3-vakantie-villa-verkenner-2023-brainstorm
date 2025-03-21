using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

namespace VillaVerkenerAPI.Endpoints;

[Route("api/villa")]
[ApiController]
public class VillaController : ControllerBase
{
    private readonly DBContext _dbContext;

    public VillaController(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-all")]
    public async Task<ActionResult<RequestResponse>> GetAllVillas()
    {
        List<Villa> villaEntities = await _dbContext.Villas.ToListAsync();
        List<SmallVilla> villas = villaEntities.Select(v => SmallVilla.From(v)).ToList();
        return Ok(RequestResponse.Successfull(data: new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villas) } }));
    }

    [HttpPost("get-by-ids")]
    public async Task<ActionResult<RequestResponse>> GetVillasByIds([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Ids are required" } }));
        }

        List<SmallVilla> villaList = await _dbContext.Villas
            .Where(v => ids.Contains(v.VillaId))
            .Select(v => SmallVilla.From(v))
            .ToListAsync();

        if (!villaList.Any())
        {
            return NotFound(RequestResponse.Failed("No villas found", new Dictionary<string, string> { { "Reason", "No villas found with the given ids" } }));
        }

        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villaList) } }));
    }

    [HttpGet("get-tags")]
    public async Task<ActionResult<RequestResponse>> GetTags()
    {
        IEnumerable<PropertyTag> PropertyTags = await _dbContext.PropertyTags.ToListAsync();
        IEnumerable<LocationTag> LocationTags = await _dbContext.LocationTags.ToListAsync();
        return Ok(RequestResponse.Successfull(data: new Dictionary<string, string> { { "PropertyTags", JsonSerializer.Serialize(PropertyTags) }, { "LocationTags", JsonSerializer.Serialize(LocationTags) } }));
    }

    public class FilterRequest
    {
        public string Search { get; set; }
        public string Location { get; set; }
        public List<string> PropertyTagsIDs { get; set; }
        public List<string> LocationTagsIDs { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int MinRooms { get; set; }
        public int MaxRooms { get; set; }
        public int MinBathrooms { get; set; }
        public int MaxBathrooms { get; set; }
        public int MinGuests { get; set; }
        public int MaxGuests { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    [HttpPost("get-by-filters")]
    public async Task<ActionResult<RequestResponse>> GetVillasFiltered([FromBody] FilterRequest filters)
    {
        Console.WriteLine(filters);

        List<SmallVilla> filteredVillas = await _dbContext.Villas
            .Where(v => filters.Search != null ? v.Naam.Contains(filters.Search) : true)
            .Where(v => filters.Location != null ? v.Locatie.Contains(filters.Location) : true)
            .Where(v => filters.MinPrice > 0 ? v.Prijs >= filters.MinPrice : true)
            .Where(v => filters.MaxPrice > 0 ? v.Prijs <= filters.MaxPrice : true)
            .Where(v => filters.MinRooms > 0 ? v.Slaapkamers >= filters.MinRooms : true)
            .Where(v => filters.MaxRooms > 0 ? v.Slaapkamers <= filters.MaxRooms : true)
            .Where(v => filters.MinBathrooms > 0 ? v.Badkamers >= filters.MinBathrooms : true)
            .Where(v => filters.MaxBathrooms > 0 ? v.Badkamers <= filters.MaxBathrooms : true)
            .Where(v => filters.MinGuests > 0 ? v.Capaciteit >= filters.MinGuests : true)
            .Where(v => filters.MaxGuests > 0 ? v.Capaciteit <= filters.MaxGuests : true)
            .Where(v => filters.PropertyTagsIDs != null && filters.PropertyTagsIDs.Any() ? _dbContext.VillaPropertyTags.Where(vpt => filters.PropertyTagsIDs.Contains(vpt.PropertyTagId.ToString())).Select(vpt => vpt.VillaId).Contains(v.VillaId) : true)
            .Where(v => filters.LocationTagsIDs != null && filters.LocationTagsIDs.Any() ? _dbContext.VillaLocationTags.Where(vlt => filters.LocationTagsIDs.Contains(vlt.LocationTagId.ToString())).Select(vlt => vlt.VillaId).Contains(v.VillaId) : true)
            .Select(v => SmallVilla.From(v))
            .ToListAsync();


        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(filteredVillas) } }));
    }
}
