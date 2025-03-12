using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;

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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Villa>>> GetAllVillas()
    {
        return await _dbContext.Villas.ToListAsync();
    }

    [HttpGet("{ids}")]
    public async Task<ActionResult<IEnumerable<SmallVilla>>> GetVillasByIds([FromRoute] string ids)
    {
        List<int> idList = ids.Split(',').Select(int.Parse).ToList();
        
        List<SmallVilla> villaList = new List<SmallVilla>();
        await _dbContext.Villas.Where(v => idList.Contains(v.VillaId)).ForEachAsync(villa => villaList.Add(SmallVilla.From(villa)));


        if (!villaList.Any()) return NotFound();
        return villaList;
    }

}
