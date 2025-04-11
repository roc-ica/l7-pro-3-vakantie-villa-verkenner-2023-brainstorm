using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

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

        string fileName = $"flyer_{villa.Naam.Trim().Replace(" ", "_")}.pdf";
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "PDF", fileName);
        if (System.IO.File.Exists(outputPath))
        {
            return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "PDF", APIUrlHandler.GetPDFUrl(fileName) } }));
        }

        villa.Images = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();

        try
        {
            PDFGenerate PDFGenerate = new();
            RequestResponse result = PDFGenerate.Main(villa, outputPath, shouldRegenerate: false);

            if (result.Success == false)
            {
                return BadRequest(RequestResponse.Failed("PDF generation failed", new Dictionary<string, string> { { "Reason", result.Message } }));
            }
            return Ok(RequestResponse.Successfull("PDF generated successfully", new Dictionary<string, string> { { "PDF", APIUrlHandler.GetPDFUrl(fileName) }, { "innerMessage", result.Message } }));

        }
        catch (Exception ex)
        {
            return BadRequest(RequestResponse.Failed("PDF generation failed", new Dictionary<string, string> { { "Reason", ex.Message } }));
        }
    }
}


