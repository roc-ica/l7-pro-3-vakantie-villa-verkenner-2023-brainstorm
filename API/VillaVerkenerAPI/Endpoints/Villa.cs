using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        List<Villa> villaEntities = await _dbContext.Villas.Where(v => v.IsDeleted == 0).ToListAsync();
        List<SmallVilla> villas = villaEntities.Select(v => SmallVilla.From(v)).ToList();
        return Ok(RequestResponse.Successfull(data: new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villas) } }));
    }

    [HttpGet("get-all-admin")]
    public async Task<ActionResult<RequestResponse>> GetAllAdminVillas([FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        bool isAllowed = await AdminController.IsValidAuth(authorizationHeader, _dbContext);
        if (!isAllowed)
        {
            return Unauthorized(RequestResponse.Failed("Unauthorized", new Dictionary<string, string> { { "Reason", "Invalid authorization header" } }));
        }

        List<Villa> villaEntities = await _dbContext.Villas.Where(v => v.IsDeleted == 0).ToListAsync();
        List<Request> requests = await _dbContext.Requests.Where(r => r.IsDeleted == 0).ToListAsync();
        List<AdminVilla> villas = villaEntities.Select(v =>
        {
            v.Images = _dbContext.Images.Where(i => i.VillaId == v.VillaId).ToList();
            List<AdminRequest> requestsForVilla = requests
                .Where(r => r.VillaId == v.VillaId)
                .Where(r => r.IsDeleted == 0)
                .Select(r => AdminRequest.From(r))
                .ToList();
            return AdminVilla.From(v, requestsForVilla);
        }).ToList();
        return Ok(RequestResponse.Successfull(data: new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villas) } }));
    }

    [HttpPost("get-first")]
    public async Task<ActionResult<RequestResponse>> GetFirstVillas([FromBody] int count)
    {
        if (count <= 0)
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Count must be greater than 0" } }));
        }

        List<Villa> villaEntities = await _dbContext.Villas
            .Where(v => v.IsDeleted == 0)
            .Take(count)
            .ToListAsync();

        foreach (Villa villa in villaEntities)
        {
            villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
        }

        List<SmallVilla> villaList = villaEntities
            .Select(v => SmallVilla.From(v))
            .ToList();

        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villaList) } }));
    }

    [HttpPost("get-by-ids")]
    public async Task<ActionResult<RequestResponse>> GetVillasByIds([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Ids are required" } }));
        }

        List<Villa> villaEntities = await _dbContext.Villas
            .Where(v => v.IsDeleted == 0)
            .Where(v => ids.Contains(v.VillaId))
            .ToListAsync();

        foreach (Villa villa in villaEntities)
        {
            villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
        }

        List<SmallVilla> villaList = villaEntities
            .Select(v => SmallVilla.From(v))
            .ToList();

        if (!villaList.Any())
        {
            return NotFound(RequestResponse.Failed("No villas found", new Dictionary<string, string> { { "Reason", "No villas found with the given ids" } }));
        }

        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(villaList) } }));
    }

    [HttpPost("get-by-id")]
    public async Task<ActionResult<RequestResponse>> GetVillaById([FromBody] int id)
    {
        if (id <= 0)
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Id is required" } }));
        }
        Villa? villa = await _dbContext.Villas
            .Where(v => v.VillaId == id)
            .Where(v => v.IsDeleted == 0)
            .FirstOrDefaultAsync();
        if (villa == null)
        {
            return NotFound(RequestResponse.Failed("No villa found", new Dictionary<string, string> { { "Reason", "No villa found with the given id" } }));
        }
        villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
        DetailedVilla smallVilla = DetailedVilla.From(villa);
        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villa", JsonSerializer.Serialize(smallVilla) } }));
    }

    [HttpPost("get-by-id-edit")]
    public async Task<ActionResult<RequestResponse>> GetVillaByIdEdit([FromBody] int id, [FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        bool isAllowed = await AdminController.IsValidAuth(authorizationHeader, _dbContext);
        if (!isAllowed)
        {
            return Unauthorized(RequestResponse.Failed("Unauthorized", new Dictionary<string, string> { { "Reason", "Invalid authorization header" } }));
        }
        if (id <= 0)
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Id is required" } }));
        }
        Villa? villa = await _dbContext.Villas
            .Where(v => v.VillaId == id)
            .Where(v => v.IsDeleted == 0)
            .FirstOrDefaultAsync();
        if (villa == null)
        {
            return NotFound(RequestResponse.Failed("No villa found", new Dictionary<string, string> { { "Reason", "No villa found with the given id" } }));
        }
        villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
        villa.VillaPropertyTags = await _dbContext.VillaPropertyTags.Where(vpt => vpt.VillaId == villa.VillaId).ToListAsync();
        villa.VillaLocationTags = await _dbContext.VillaLocationTags.Where(vlt => vlt.VillaId == villa.VillaId).ToListAsync();
        EditVilla smallVilla = EditVilla.From(villa, _dbContext);
        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villa", JsonSerializer.Serialize(smallVilla) } }));
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
            .Where(v => v.IsDeleted == 0)
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

        filteredVillas.ForEach(villa =>
        {
            Image? primaryImage = _dbContext.Images
                .Where(i => i.VillaId == villa.VillaID && i.IsPrimary == 1)
                .FirstOrDefault();
            villa.VillaImagePath = APIUrlHandler.GetImageUrl(primaryImage?.ImageLocation ?? "");
        });

        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villas", JsonSerializer.Serialize(filteredVillas) } }));
    }

    [HttpPost("delete")]
    public async Task<ActionResult<RequestResponse>> deleteVilla([FromBody] int id, [FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        bool isAllowed = await AdminController.IsValidAuth(authorizationHeader, _dbContext);
        if (!isAllowed)
        {
            return Unauthorized(RequestResponse.Failed("Unauthorized", new Dictionary<string, string> { { "Reason", "Invalid authorization header" } }));
        }
        Villa? villa = await _dbContext.Villas
            .Where(v => v.VillaId == id)
            .Where(v => v.IsDeleted == 0)
            .FirstOrDefaultAsync();
        if (villa == null)
        {
            return NotFound(RequestResponse.Failed("No villa found", new Dictionary<string, string> { { "Reason", "No villa found with the given id" } }));
        }
        villa.IsDeleted = 1;
        villa.DeletedAt = DateTime.Now;
        _dbContext.Villas.Update(villa);
        await _dbContext.SaveChangesAsync();
        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villa", JsonSerializer.Serialize(villa) } }));
    }

    public struct RequestBody
    {
        public int VillaID { get; set; }
        public int RequestID { get; set; }
    }

    [HttpPost("delete-request")]
    public async Task<ActionResult<RequestResponse>> deleteRequest([FromBody] RequestBody body, [FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        bool isAllowed = await AdminController.IsValidAuth(authorizationHeader, _dbContext);
        if (!isAllowed)
        {
            return Unauthorized(RequestResponse.Failed("Unauthorized", new Dictionary<string, string> { { "Reason", "Invalid authorization header" } }));
        }
        Request? request = await _dbContext.Requests
            .Where(r => r.RequestId == body.RequestID)
            .Where(r => r.VillaId == body.VillaID)
            .Where(r => r.IsDeleted == 0)
            .FirstOrDefaultAsync();
        if (request == null)
        {
            return NotFound(RequestResponse.Failed("No request found", new Dictionary<string, string> { { "Reason", "No request found with the given id" } }));
        }

        request.IsDeleted = 1;
        request.DeletedAt = DateTime.Now;
        _dbContext.Requests.Update(request);
        await _dbContext.SaveChangesAsync();

        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "Villa", JsonSerializer.Serialize(body.VillaID) }, { "Request", JsonSerializer.Serialize(body.RequestID) } }));
    }
}