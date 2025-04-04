using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;
using VillaVerkenerAPI.PDF;

namespace VillaVerkenerAPI.Endpoints;

[Route("api/PDF")]
[ApiController]
public class PDFController : ControllerBase
{
    private readonly DBContext _dbContext;

    public PDFController(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("get")]
    public async Task<ActionResult<RequestResponse>> PDFGenerator([FromBody] int id)
    {
        Villa? villa = await _dbContext.Villas
            .FirstOrDefaultAsync(v => v.VillaId == id);

        if (villa == null)
        {
            return NotFound(RequestResponse.Failed("Villa not found", new Dictionary<string, string> { { "Reason", "No villa found with the given id" } }));
        }

        villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
        
        try
        {
            PDFGenerate PDFGenerate = new PDFGenerate();
            PDFGenerate.Main(villa);
        }
        catch (Exception ex)
        {
            return BadRequest(RequestResponse.Failed("PDF generation failed", new Dictionary<string, string> { { "Reason", ex.Message } }));
        }
        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "PDF", "PDF Generated" }, { "id", id.ToString() } }));
    }
}


