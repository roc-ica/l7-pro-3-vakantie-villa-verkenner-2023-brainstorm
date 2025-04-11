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
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        List<string> createdFiles = new List<string>();
        string GlobalFolder = "";
        string GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Images", GlobalFolder);
        try
        {

            Villa? villa = await _dbContext.Villas.FirstOrDefaultAsync(v => v.VillaId == uploadVillaRequest.VillaId);

            if (villa == null)
            {
                return NotFound(RequestResponse.Failed("Villa not found", new() { { "Reason", "Invalid VillaId" } }));
            }

            List<string> validationErrors = uploadVillaRequest.Validate(_dbContext);
            if (validationErrors.Count != 0)
            {
                string errorMessage = string.Join("\n", validationErrors);
                return BadRequest(RequestResponse.Failed("Invalid input", new() { { "Reason", errorMessage } }));
            }
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

            List<Image> existingImages = await _dbContext.Images.Where(i => i.VillaId == villa.VillaId).ToListAsync();
            string? folder = Path.GetDirectoryName(existingImages.FirstOrDefault(i => i.IsPrimary == 1)?.ImageLocation);
            if (folder == null)
            {
                foreach (Image image in existingImages)
                {
                    if (image.IsPrimary == 0)
                    {
                        folder = Path.GetDirectoryName(image.ImageLocation);
                        break;
                    }
                }
            }
            folder ??= Path.Combine(Directory.GetCurrentDirectory(), "Images", $"{Guid.NewGuid()}");
            GlobalFolder = folder;
            GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Images", GlobalFolder);
            Directory.CreateDirectory(folder);

            List<string> deletedImages = uploadVillaRequest.RemovedImagesJson != null ? JsonSerializer.Deserialize<List<string>>(uploadVillaRequest.RemovedImagesJson) ?? new List<string>() : new List<string>();
            foreach (string imageUrl in deletedImages)
            {
                string[] pathParts = imageUrl.Split(Path.DirectorySeparatorChar);
                string location = Path.Combine(pathParts[^2], pathParts[^1]);
                await DeleteImage(location);
            }
            Console.WriteLine("--------------DELETED IMAGES--------------------");
            foreach (IFormFile image in uploadVillaRequest.Images)
            {
                Image uploadedImage = await UploadImage(image, GlobalLocation, folder, createdFiles);
                uploadedImage.VillaId = villa.VillaId;
                _dbContext.Images.Add(uploadedImage);
            }


            // Upload main image
            Image? currentMainImage = _dbContext.Images
                    .Where(i => i.VillaId == villa.VillaId && i.IsPrimary == 1)
                    .FirstOrDefault();
            if (currentMainImage != null)
            {
                currentMainImage.IsPrimary = 0;
                _dbContext.Images.Update(currentMainImage);
                await _dbContext.SaveChangesAsync();
            }

            if (uploadVillaRequest.MainImage != null)
            {
                Image uploadedPrimaryImage = await UploadImage(uploadVillaRequest.MainImage, GlobalLocation, folder, createdFiles);
                uploadedPrimaryImage.VillaId = villa.VillaId;
                uploadedPrimaryImage.IsPrimary = 1;
                _dbContext.Images.Add(uploadedPrimaryImage);
                await _dbContext.SaveChangesAsync();
            }
            else if (uploadVillaRequest.MainImageUrl != null)
            {
                Image? existingImage = await _dbContext.Images
                    .Where(i => i.ImageLocation.Equals(uploadVillaRequest.GetLocation()))
                    .FirstOrDefaultAsync();
                if (existingImage != null)
                {
                    existingImage.IsPrimary = 1;
                    _dbContext.Images.Update(existingImage);
                    await _dbContext.SaveChangesAsync();
                }
            }
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("------------ADDED NEW IMAGES---------------");

            Image? newMainImage = await _dbContext.Images
                .Where(i => i.VillaId == villa.VillaId && i.IsPrimary == 1)
                .FirstOrDefaultAsync();
            if (newMainImage is null)
            {
                Console.WriteLine("setting old primary image back");
                currentMainImage.IsPrimary = 1;
                _dbContext.Images.Update(currentMainImage);
                await _dbContext.SaveChangesAsync();
            }


            List<VillaPropertyTag> existingPropertyTags = await _dbContext.VillaPropertyTags
                .Where(vpt => vpt.VillaId == villa.VillaId)
                .ToListAsync();
            _dbContext.VillaPropertyTags.RemoveRange(existingPropertyTags);
            await _dbContext.SaveChangesAsync();

            List<VillaPropertyTag> propertyTags = uploadVillaRequest.PropertyTags
                .Select(tagId => new VillaPropertyTag { VillaId = villa.VillaId, PropertyTagId = tagId })
                .ToList();
            await _dbContext.VillaPropertyTags.AddRangeAsync(propertyTags);
            await _dbContext.SaveChangesAsync();

            List<VillaLocationTag> existingLocationTags = await _dbContext.VillaLocationTags
                .Where(vlt => vlt.VillaId == villa.VillaId)
                .ToListAsync();
            _dbContext.VillaLocationTags.RemoveRange(existingLocationTags);
            await _dbContext.SaveChangesAsync();

            List<VillaLocationTag> locationTags = uploadVillaRequest.LocationTags
                .Select(tagId => new VillaLocationTag { VillaId = villa.VillaId, LocationTagId = tagId })
                .ToList();
            await _dbContext.VillaLocationTags.AddRangeAsync(locationTags);
            await _dbContext.SaveChangesAsync();

            transaction.Commit();
            return RequestResponse.Successfull("Success", new() { { "data", System.Text.Json.JsonSerializer.Serialize(uploadVillaRequest) } });

        }
        catch (Exception ex)
        {
            Console.WriteLine("+-+-+-+-+-+-+-+-+-+-EROR+-+-+-+-+-+-+-+-");
            await transaction.RollbackAsync();

            foreach (string file in createdFiles)
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }

            if (Directory.Exists(GlobalLocation) && !Directory.EnumerateFileSystemEntries(GlobalLocation).Any())
            {
                Directory.Delete(GlobalLocation);
            }

            return BadRequest(RequestResponse.Failed("Failed", new() { { "Reason", ex.Message } }));
        }
    }
}
