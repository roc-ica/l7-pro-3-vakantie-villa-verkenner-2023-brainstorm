using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaVerkenerAPI.Models;
using VillaVerkenerAPI.Models.DB;
using VillaVerkenerAPI.Services;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace VillaVerkenerAPI.Endpoints;

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
        string path = await ImageUploader.UploadImage(image, location, folder, createdFiles);

        return new Image
        {
            ImageLocation = path
        };
    }

    private async Task<bool> DeleteImage(string location)
    {
        Image? image = await _dbContext.Images.FirstOrDefaultAsync(i => i.ImageLocation.Equals(location));
        if (image != null)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", image.ImageLocation);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            else
            {
                Console.WriteLine($"--{filePath} not deleted--");
            }
            _dbContext.Images.Remove(image);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        else
        {
            Console.WriteLine($"--{location} not found--");
        }
            return false;
    }

    [HttpPost("upload-villa")]
    public async Task<ActionResult<RequestResponse>> UploadVilla([FromForm] UploadAddVillaRequest uploadVillaRequest)
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

            PDFGenerate pdfGenerate = new PDFGenerate();
            string pdfPath = Path.Combine(location, $"flyer_{newVillaEntity.Entity.Naam.Trim().Replace(" ", "_")}.pdf");
            RequestResponse pdfResult = pdfGenerate.Main(newVillaEntity.Entity, pdfPath, shouldRegenerate: false);

            if (pdfResult.Success == false)
            {
                return Ok(RequestResponse.Successfull("Success", new() { { "path", Directory.GetCurrentDirectory() }, { "PDFGenerated", "false" }, { "PDFPath", "" } }));
            }

            return Ok(RequestResponse.Successfull("Success", new() { { "path", Directory.GetCurrentDirectory() }, { "PDFGenerated", "true" }, { "PDFPath", pdfPath } }));
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

    [HttpPost("edit-villa")]
    public async Task<ActionResult<RequestResponse>> EditVilla([FromForm] UploadEditVillaRequest uploadVillaRequest)
    {
        Console.WriteLine("---------EDIT----------------");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var createdFiles = new List<string>();
        var imageBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
        string globalImageFolder = "";
        string fullImagePath = "";

        try
        {
            // Validate input
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.VillaId == uploadVillaRequest.VillaId);
            if (villa == null)
                return NotFound(RequestResponse.Failed("Villa not found", new() { { "Reason", "Invalid VillaId" } }));

            var validationErrors = uploadVillaRequest.Validate(_dbContext);
            if (validationErrors.Any())
            {
                var errorMessage = string.Join("\n", validationErrors);
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", errorMessage } }));
            }

            // Update villa properties
            villa.Naam = uploadVillaRequest.VillaName;
            villa.Omschrijving = uploadVillaRequest.Description;
            villa.Prijs = uploadVillaRequest.Price;
            villa.Locatie = uploadVillaRequest.Location;
            villa.Capaciteit = uploadVillaRequest.Capacity;
            villa.Slaapkamers = uploadVillaRequest.Bedrooms;
            villa.Badkamers = uploadVillaRequest.Bathrooms;

            _dbContext.Villas.Update(villa);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("--------------UPDATED VILLA -----------------");

            // Determine image folder
            var existingImages = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
            var folder = Path.GetDirectoryName(existingImages.FirstOrDefault(i => i.IsPrimary == 1)?.ImageLocation)
                        ?? Path.GetDirectoryName(existingImages.FirstOrDefault(i => i.IsPrimary == 0)?.ImageLocation)
                        ?? Path.Combine(imageBasePath, Guid.NewGuid().ToString());

            globalImageFolder = folder;
            fullImagePath = Path.Combine(imageBasePath, globalImageFolder);
            Directory.CreateDirectory(folder);

            // Handle deleted images
            var deletedImages = uploadVillaRequest.RemovedImagesJson != null
                ? JsonSerializer.Deserialize<List<string>>(uploadVillaRequest.RemovedImagesJson) ?? new List<string>()
                : new List<string>();

            foreach (var imageUrl in deletedImages)
            {
                var pathParts = imageUrl.Split(Path.DirectorySeparatorChar);
                var relativePath = Path.Combine(pathParts[^2], pathParts[^1]);
                await DeleteImage(relativePath);
            }
            Console.WriteLine("--------------DELETED IMAGES--------------------");

            // Upload new images
            foreach (var image in uploadVillaRequest.Images)
            {
                var uploaded = await UploadImage(image, fullImagePath, folder, createdFiles);
                uploaded.VillaId = villa.VillaId;
                _dbContext.Images.Add(uploaded);
            }

            // Handle main image
            var currentMain = await _dbContext.Images
                .Where(i => i.VillaId == villa.VillaId && i.IsPrimary == 1)
                .FirstOrDefaultAsync();

            if (currentMain != null)
            {
                currentMain.IsPrimary = 0;
                _dbContext.Images.Update(currentMain);
                await _dbContext.SaveChangesAsync();
            }

            if (uploadVillaRequest.MainImage != null)
            {
                var primary = await UploadImage(uploadVillaRequest.MainImage, fullImagePath, folder, createdFiles);
                primary.VillaId = villa.VillaId;
                primary.IsPrimary = 1;
                _dbContext.Images.Add(primary);
                await _dbContext.SaveChangesAsync();
            }
            else if (uploadVillaRequest.MainImageUrl != null)
            {
                var existing = await _dbContext.Images
                    .FirstOrDefaultAsync(i => i.ImageLocation == uploadVillaRequest.GetLocation());

                if (existing != null)
                {
                    existing.IsPrimary = 1;
                    _dbContext.Images.Update(existing);
                    await _dbContext.SaveChangesAsync();
                }
            }

            // Fallback if no main image set
            var newMain = await _dbContext.Images
                .FirstOrDefaultAsync(i => i.VillaId == villa.VillaId && i.IsPrimary == 1);

            if (newMain is null && currentMain is not null)
            {
                Console.WriteLine("setting old primary image back");
                currentMain.IsPrimary = 1;
                _dbContext.Images.Update(currentMain);
                await _dbContext.SaveChangesAsync();
            }

            Console.WriteLine("------------ADDED NEW IMAGES---------------");

            // Sync property tags
            var oldPropertyTags = await _dbContext.VillaPropertyTags
                .Where(vpt => vpt.VillaId == villa.VillaId)
                .ToListAsync();
            _dbContext.VillaPropertyTags.RemoveRange(oldPropertyTags);
            await _dbContext.SaveChangesAsync();

            var newPropertyTags = uploadVillaRequest.PropertyTags
                .Select(id => new VillaPropertyTag { VillaId = villa.VillaId, PropertyTagId = id })
                .ToList();
            await _dbContext.VillaPropertyTags.AddRangeAsync(newPropertyTags);
            await _dbContext.SaveChangesAsync();

            // Sync location tags
            var oldLocationTags = await _dbContext.VillaLocationTags
                .Where(vlt => vlt.VillaId == villa.VillaId)
                .ToListAsync();
            _dbContext.VillaLocationTags.RemoveRange(oldLocationTags);
            await _dbContext.SaveChangesAsync();

            var newLocationTags = uploadVillaRequest.LocationTags
                .Select(id => new VillaLocationTag { VillaId = villa.VillaId, LocationTagId = id })
                .ToList();
            await _dbContext.VillaLocationTags.AddRangeAsync(newLocationTags);
            await _dbContext.SaveChangesAsync();

            // Done
            await transaction.CommitAsync();
            return RequestResponse.Successfull("Success", new() { { "data", JsonSerializer.Serialize(uploadVillaRequest) } });
        }
        catch (Exception ex)
        {
            Console.WriteLine("+-+-+-+-+-+-+-+-+-+-EROR+-+-+-+-+-+-+-+-");
            await transaction.RollbackAsync();

            foreach (var file in createdFiles.Where(System.IO.File.Exists))
            {
                System.IO.File.Delete(file);
            }

            if (Directory.Exists(fullImagePath) && !Directory.EnumerateFileSystemEntries(fullImagePath).Any())
            {
                Directory.Delete(fullImagePath);
            }

            return BadRequest(RequestResponse.Failed("Failed", new() { { "Reason", ex.Message } }));
        }
    }
}
