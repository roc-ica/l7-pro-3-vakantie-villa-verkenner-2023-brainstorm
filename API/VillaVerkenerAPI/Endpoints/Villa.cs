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
}
