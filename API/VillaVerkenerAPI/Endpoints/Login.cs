using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

namespace VillaVerkenerAPI.Endpoints;

[Route("api/login")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly DBContext _dbContext;

    public LoginController(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{Email}/{Password}")]
    public async Task<ActionResult<RequestResponse>> GetRequest([FromRoute] string Email, [FromRoute] string Password )
    {
        User? user = await _dbContext.Users.Where(user=>user.IsDeleted == 0).FirstOrDefaultAsync(user => Email.Equals(user.Email));

        if (user == null)
        {
            return RequestResponse.Failed("Wrong", new Dictionary<string, string> { {"Reason","verkeerde Email"} });
        }

        if (PasswordHasher.ValidatePassword(Password, user.Password))
        {
            Guid sessionKey = Guid.NewGuid();
            Session NewSession = new Session();
            NewSession.SessionKey = sessionKey.ToString();
            NewSession.UserId = user.UserId;
            NewSession.ExpirationDate = DateTime.UtcNow.AddDays(7);
            await _dbContext.Sessions.AddAsync(NewSession);
            return RequestResponse.Successfull("Success", new Dictionary<string, string> { { "SessionKey", sessionKey.ToString() } });
        }
        else
        {
            return RequestResponse.Failed("Wrong", new Dictionary<string, string> { { "Reason", "verkeerde Wachtwoord" } });
        }
    }
}
