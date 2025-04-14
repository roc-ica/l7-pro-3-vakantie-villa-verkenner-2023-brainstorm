using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

namespace VillaVerkenerAPI.Models;

public class MoreInfoRequest
{
    public required int VillaId { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }
}

[Route("api/moreInfoRequest")]
[ApiController]
public class MoreInfoRequestController(DBContext dbContext) : ControllerBase
{
    private readonly DBContext _dbContext = dbContext;

    [HttpPost("moreInfoRequest")]
    public async Task<ActionResult<RequestResponse>> RequestMoreInfo([FromBody] MoreInfoRequest moreInfoRequest)
    {
        if (string.IsNullOrEmpty(moreInfoRequest.Email) || string.IsNullOrEmpty(moreInfoRequest.Message))
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Email and Message are required" } }));
        }

        if (!IsValidEmail(moreInfoRequest.Email))
        {
            return BadRequest(RequestResponse.Failed("Invalid email", new Dictionary<string, string> { { "Reason", "Invalid email" } }));
        }

        if (moreInfoRequest.VillaId == null)
        {
            return BadRequest(RequestResponse.Failed("Invalid villa id", new Dictionary<string, string> { { "Reason", "Villa id is required" } }));
        }

        Request request = new()
        {
            VillaId = moreInfoRequest.VillaId,
            Email = moreInfoRequest.Email,
            Message = moreInfoRequest.Message
        };

        await _dbContext.Requests.AddAsync(request);
        await _dbContext.SaveChangesAsync();

        return Ok(RequestResponse.Successfull("Request for more info successfull!"));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            MailAddress m = new(email);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}