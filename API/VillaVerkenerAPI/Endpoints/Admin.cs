using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;

namespace VillaVerkenerAPI.Endpoints;
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly DBContext _dbContext;

    public AdminController(DBContext dbContext)
    {
        _dbContext = dbContext;
    }

    private async Task<bool> IsValidSession(string sessionKey)
    {
        Session? session = await _dbContext.Sessions
            .Where(session => session.ExpirationDate > DateTime.UtcNow)
            .FirstOrDefaultAsync(session => session.SessionKey.Equals(sessionKey));
        return session != null;
    }

    [HttpGet("is-allowed")]
    public async Task<ActionResult<RequestResponse>> IsAllowed([FromHeader(Name = "Authorization")] string authorizationHeader)
    {
        string sessionKey = authorizationHeader.Split(" ")[1];
        bool isValid = await IsValidSession(sessionKey);
        if (isValid)
        {
            return Ok(RequestResponse.Successfull("Success"));
        }
        else
        {
            return Unauthorized(RequestResponse.Failed("Unauthorized"));
        }
    }


    [HttpPost("login")]
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
    public class UploadVillaRequest
    {
        public string VillaName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public List<IFormFile> Images { get; set; }
        public IFormFile MainImage { get; set; }

        public string PropertyTagsJson { get; set; }
        public List<int> PropertyTags
        {
            get
            {
                if (string.IsNullOrEmpty(PropertyTagsJson))
                {
                    return new List<int>();
                }
                else
                {
                    List<string> tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(PropertyTagsJson)?.ToList() ?? new List<string>();
                    List<int> tagIds = new List<int>();
                    foreach (string tag in tags)
                    {
                        if (int.TryParse(tag, out int tagId))
                        {
                            tagIds.Add(tagId);
                        }
                    }
                    return tagIds;
                }
            }
        }
        public string LocationTagsJson { get; set; }
        public List<int> LocationTags
        {
            get
            {
                if (string.IsNullOrEmpty(LocationTagsJson))
                {
                    return new List<int>();
                }
                else
                {
                    List<string> tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(LocationTagsJson)?.ToList() ?? new List<string>();
                    List<int> tagIds = new List<int>();
                    foreach (string tag in tags)
                    {
                        if (int.TryParse(tag, out int tagId))
                        {
                            tagIds.Add(tagId);
                        }
                    }
                    return tagIds;
                }
            }
        }
    }

    private async Task<Image> UploadImage(IFormFile image, string location, string folder, List<string> createdFiles)
    {
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        string path = Path.Combine(location, fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        createdFiles.Add(path);

        return new Image
        {
            ImageLocation = Path.Combine(folder, fileName),
        };
    }

    [HttpPost("upload-villa")]
    public async Task<ActionResult<RequestResponse>> UploadVilla([FromForm] UploadVillaRequest uploadVillaRequest)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        List<string> createdFiles = new List<string>(); // Track created files
        string folder = Path.Combine($"{Guid.NewGuid()}-{uploadVillaRequest.VillaName}");
        string location = Path.Combine(Directory.GetCurrentDirectory(), "Images", folder);

        try
        {
            if (uploadVillaRequest.Images == null || uploadVillaRequest.Images.Count == 0)
            {
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", "Images are required" } }));
            }
            if (uploadVillaRequest.MainImage == null)
            {
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", "Main Image is required" } }));
            }

            if (_dbContext.Villas.Any(v => v.Naam == uploadVillaRequest.VillaName))
            {
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", "Villa with this name already exists" } }));
            }

            Villa newVilla = new Villa
            {
                Naam = uploadVillaRequest.VillaName,
                Omschrijving = uploadVillaRequest.Description,
                Prijs = uploadVillaRequest.Price,
                Locatie = uploadVillaRequest.Location,
                Capaciteit = uploadVillaRequest.Capacity,
                Slaapkamers = uploadVillaRequest.Bedrooms,
                Badkamers = uploadVillaRequest.Bathrooms
            };

            var newVillaEntity = await _dbContext.Villas.AddAsync(newVilla);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Saved villa");

            Directory.CreateDirectory(location);

            List<Image> images = new List<Image>();

            foreach (IFormFile image in uploadVillaRequest.Images)
            {
                Image uploadedImage = await UploadImage(image, location, folder, createdFiles);
                uploadedImage.VillaId = newVillaEntity.Entity.VillaId;
                images.Add(uploadedImage);
            }

            // Upload main image
            Image uploadedPrimaryImage = await UploadImage(uploadVillaRequest.MainImage, location, folder, createdFiles);
            uploadedPrimaryImage.VillaId = newVillaEntity.Entity.VillaId;
            uploadedPrimaryImage.IsPrimary = 1;
            images.Add(uploadedPrimaryImage);

            await _dbContext.Images.AddRangeAsync(images);
            await _dbContext.SaveChangesAsync();

            List<VillaPropertyTag> propertyTags = uploadVillaRequest.PropertyTags
                .Select(tagId => new VillaPropertyTag { VillaId = newVillaEntity.Entity.VillaId, PropertyTagId = tagId })
                .ToList();

            List<VillaLocationTag> locationTags = uploadVillaRequest.LocationTags
                .Select(tagId => new VillaLocationTag { VillaId = newVillaEntity.Entity.VillaId, LocationTagId = tagId })
                .ToList();

            await _dbContext.VillaPropertyTags.AddRangeAsync(propertyTags);
            await _dbContext.VillaLocationTags.AddRangeAsync(locationTags);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(RequestResponse.Successfull("Success", new() { { "path", Directory.GetCurrentDirectory() } }));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();

            foreach (string file in createdFiles)
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }

            if (Directory.Exists(location) && !Directory.EnumerateFileSystemEntries(location).Any())
            {
                Directory.Delete(location);
            }

            return BadRequest(RequestResponse.Failed("Failed", new() { { "Reason", e.Message } }));
        }
    }
}
