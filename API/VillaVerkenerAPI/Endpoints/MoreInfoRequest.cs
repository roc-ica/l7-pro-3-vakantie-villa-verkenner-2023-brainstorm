using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;
using static VillaVerkenerAPI.Endpoints.MoreInfoRequestController;

namespace VillaVerkenerAPI.Endpoints;

[Route("api/moreInfoRequest")]
[ApiController]
public class MoreInfoRequestController(DBContext dbContext) : ControllerBase
{
    private readonly DBContext _dbContext = dbContext;

    public class MoreInfoRequest
    {
        public required int VillaId { get; set; }
        public required string Email { get; set; }
        public required string Message { get; set; }
    }

    [HttpPost("moreInfoRequest")]
    public async Task<ActionResult<RequestResponse>> RequestMoreInfo([FromBody] MoreInfoRequest moreInfoRequest)
    {
        if (string.IsNullOrEmpty(moreInfoRequest.Email) || string.IsNullOrEmpty(moreInfoRequest.Message))
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Email and Message are required" } }));
        }

        Request request = new()
        {
            VillaId = moreInfoRequest.VillaId,
            Email = moreInfoRequest.Email,
            Message = moreInfoRequest.Message
        };

        await _dbContext.Requests.AddAsync(request);
        await _dbContext.SaveChangesAsync();

        return Ok(RequestResponse.Successfull("SUCCESS", new Dictionary<string, string> { { moreInfoRequest.Email, moreInfoRequest.Message } }));
    }
}