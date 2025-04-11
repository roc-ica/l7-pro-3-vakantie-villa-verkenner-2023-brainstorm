using System.Text.Json;
using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models;
public class UploadEditVillaRequest
{
    public int VillaId { get; set; }

    public string VillaName { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }

    public List<IFormFile> Images { get; set; } = new List<IFormFile>(); // Only NEW images
    public IFormFile? MainImage { get; set; }
    public string? MainImageUrl { get; set; }

    public string RemovedImagesJson { get; set; } // URLs of removed old images

    public string PropertyTagsJson { get; set; }
    public List<int> PropertyTags => DeserializeTags(PropertyTagsJson);

    public string LocationTagsJson { get; set; }
    public List<int> LocationTags => DeserializeTags(LocationTagsJson);


    

    public string GetLocation()
    {
        string[] pathParts = MainImageUrl.Split(System.IO.Path.DirectorySeparatorChar);
        return System.IO.Path.Combine(pathParts[^2], pathParts[^1]);
    }
    public string GetPath()
    {
        return System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Images", GetLocation());
    }

    private List<int> DeserializeTags(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<int>();
        List<string> tags = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
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
    public bool ValidateImages(DBContext _dbContext)
    {
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".avif", ".webp" };
        //make sure either main image or main image url is provided
        if (MainImage == null && string.IsNullOrEmpty(MainImageUrl))
        {
            return false;
        }
        if (Images != null && Images.Count > 0)
        {
            if (Images.Count > 20)
            {
                return false;
            }
            foreach (IFormFile image in Images)
            {
                if (image.Length > 5 * 1024 * 1024)
                {
                    return false;
                }
                string ext = System.IO.Path.GetExtension(image.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    return false;
                }
            }
        }
        if (MainImage != null)
        {
            if (MainImage.Length > 5 * 1024 * 1024)
            {
                return false;
            }
            string ext = System.IO.Path.GetExtension(MainImage.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                return false;
            }
        }

        // image counts
        int imageCount = 0;
        int ExistingImagesCount = _dbContext.Images
            .Where(i => i.VillaId == VillaId && i.IsPrimary == 0).Count();
        imageCount += ExistingImagesCount;
        imageCount += Images?.Count ?? 0;
        imageCount += MainImage != null ? 1 : 0; // add 1 if a new main image is provided
        imageCount -= RemovedImagesJson != null ? JsonSerializer.Deserialize<List<string>>(RemovedImagesJson)?.Count ?? 0 : 0; // subtract removed images
        if (imageCount > 20)
        {
            return false;
        }
        // check if main image is in the list of removed images
        if (MainImageUrl != null)
        {
            List<string> removedImages = JsonSerializer.Deserialize<List<string>>(RemovedImagesJson) ?? new List<string>();
            if (removedImages.Contains(MainImageUrl))
            {
                return false;
            }
        }

        return true;
    }


    public List<string> Validate(DBContext _dbContext)
    {
        List<string> errors = new List<string>();

        if (VillaId <= 0 || !_dbContext.Villas.Any(v => v.VillaId == VillaId))
        {
            errors.Add("Invalid Villa ID.");
        }

        if (string.IsNullOrEmpty(VillaName) || VillaName.Length > 45)
        {
            errors.Add("Villa Name must be between 1 and 45 characters.");
        }

        if (string.IsNullOrEmpty(Description))
        {
            errors.Add("Description must be more than 1 character.");
        }

        if (ValidateImages(_dbContext) == false)
        {
            errors.Add("Invalid image given");
        }


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

public class UploadAddVillaRequest
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

        if (string.IsNullOrEmpty(Description))
        {
            errors.Add("Description must be more than 1 character.");
        }
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".avif", ".webp" };
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
                        errors.Add($"Invalid image format. Allowed formats: {string.Join(",", allowedExtensions).Replace(".", "")}");
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
                errors.Add($"Invalid image format. Allowed formats: {string.Join(",", allowedExtensions).Replace(".", "")}");
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
