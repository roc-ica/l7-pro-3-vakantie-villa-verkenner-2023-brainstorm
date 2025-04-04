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
    public List<int> PropertyTags { get { return DeserializeTags(PropertyTagsJson); } }
    public string LocationTagsJson { get; set; }
    public List<int> LocationTags { get { return DeserializeTags(LocationTagsJson); } }

    // Helper method for deserializing tags from JSON
    private List<int> DeserializeTags(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<int>();
        List<string> tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
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

    // Validation method
    public List<string> Validate(DBContext _dbContext)
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrEmpty(VillaName) || VillaName.Length > 45)
        {
            errors.Add("Villa Name must be between 1 and 45 characters.");
        }

        if (string.IsNullOrEmpty(Description) || Description.Length > 128)
        {
            errors.Add("Description must be between 1 and 128 characters.");
        }
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".avif" };
        if (Images == null || Images.Count == 0)
        {
            errors.Add("Images are required.");
        }
        else 
        {
            if (Images.Count > 20)
            {
                errors.Add("You can upload a maximum of 20 images.");
            }
            else
            {
                foreach (IFormFile image in Images)
                {
                    if (image.Length > 5 * 1024 * 1024) // 5 MB limit
                    {
                        errors.Add("Image size should not exceed 5 MB.");
                    }
                    string fileExtension = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        errors.Add("Invalid image format. Allowed formats: jpg, jpeg, png, avif.");
                    }

                }
            }
        }
        if (MainImage == null)
        {
            errors.Add("Main Image is required.");
        }
        else
        {
            if (MainImage.Length > 5 * 1024 * 1024) // 5 MB limit
            {
                errors.Add("Main Image size should not exceed 5 MB.");
            }
            
            string fileExtension = Path.GetExtension(MainImage.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                errors.Add("Invalid main image format. Allowed formats: jpg, jpeg, png, avif.");
            }
        }

        if (_dbContext.Villas.Any(v => v.Naam == VillaName))
        {
            errors.Add("Villa with this name already exists.");
        }

        // Validate PropertyTags presence
        if (PropertyTags.Count == 0)
        {
            errors.Add("Property Tags are required.");
        }

        if (LocationTags.Count == 0)
        {
            errors.Add("Location Tags are required.");
        }

        if (Price <= 0)
        {
            errors.Add("Price should be greater than 0.");
        }

        if (Capacity <= 0)
        {
            errors.Add("Capacity should be greater than 0.");
        }

        if (Bedrooms <= 0)
        {
            errors.Add("Bedrooms should be greater than 0.");
        }

        if (Bathrooms <= 0)
        {
            errors.Add("Bathrooms should be greater than 0.");
        }

        return errors;
    }
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
  
    private async Task<Image> UploadImage(IFormFile image, string location, string folder, List<string> createdFiles)
    {
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        string path = Path.Combine(location, fileName);
        using (FileStream stream = new FileStream(path, FileMode.Create))
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
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        List<string> createdFiles = new List<string>();
        string folder = Path.Combine($"{Guid.NewGuid()}");
        string location = Path.Combine(Directory.GetCurrentDirectory(), "Images", folder);

        try
        {
            List<string> validationErrors = uploadVillaRequest.Validate(_dbContext);

            if (validationErrors.Count != 0)
            {
                string errorMessage = string.Join("\n", validationErrors);
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", errorMessage } }));
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

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Villa> newVillaEntity = await _dbContext.Villas.AddAsync(newVilla);
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
