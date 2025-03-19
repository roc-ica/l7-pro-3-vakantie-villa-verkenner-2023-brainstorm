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

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest(RequestResponse.Failed("Invalid input", new Dictionary<string, string> { { "Reason", "Email and Password are required" } }));
        }

        User? user = await _dbContext.Users
            .Where(user => user.IsDeleted == 0)
            .FirstOrDefaultAsync(user => loginRequest.Email.Equals(user.Email));

        if (user == null)
        {
            return Unauthorized(RequestResponse.Failed("Wrong", new Dictionary<string, string> { { "Reason", "Incorrect Email" } }));
        }

        if (PasswordHasher.ValidatePassword(loginRequest.Password, user.Password))
        {
            Guid sessionKey = Guid.NewGuid();
            Session newSession = new Session
            {
                SessionKey = sessionKey.ToString(),
                UserId = user.UserId,
                ExpirationDate = DateTime.UtcNow.AddDays(7)
            };

            await _dbContext.Sessions.AddAsync(newSession);
            await _dbContext.SaveChangesAsync();

            return Ok(RequestResponse.Successfull("Success", new Dictionary<string, string> { { "SessionKey", sessionKey.ToString() } }));
        }
        else
        {
            return Unauthorized(RequestResponse.Failed("Wrong", new Dictionary<string, string> { { "Reason", "Incorrect Password" } }));
        }
    }
}

