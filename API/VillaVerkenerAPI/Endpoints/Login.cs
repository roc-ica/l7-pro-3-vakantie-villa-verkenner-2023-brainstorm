using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;

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
    public async Task<ActionResult<bool>> GetRequest([FromRoute] string Email, [FromRoute] string Password )
    {
        User? user = await _dbContext.Users.Where(user=>user.IsDeleted == 0).FirstOrDefaultAsync(user => Email.Equals(user.Email));

        if (user == null)
        {
            return false;
        }

        if (Password.Equals(user.Password))
        {
            Guid guid = Guid.NewGuid();
            var NewSession = new Session();
            NewSession.Session1 = guid.ToString();
            NewSession.UserId = user.UserId;
            NewSession.ExpirationDate = DateTime.UtcNow.AddDays(7);
            await _dbContext.Sessions.AddAsync(NewSession);
            return true;
        }
        else
        {
            return false;
        }
        



        }


}
