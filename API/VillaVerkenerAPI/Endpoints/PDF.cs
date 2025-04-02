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

    [HttpPost("Generate")]
    public async Task<ActionResult<RequestResponse>> PDFGenerator()
    {
        Console.WriteLine("Generating PDF");
        var pdf = new PDFGenerate();
        pdf.Main();
        Console.WriteLine("PDF Generated");
        return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "PDF", "PDF Generated" } }));
    }


    }


