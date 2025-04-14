using ImageMagick;

namespace VillaVerkenerAPI.Services
{
    public static class ImageUploader
    {
        public static async Task<string> UploadImage(IFormFile image, string location, string folder, List<string> createdFiles)
        {
            using MemoryStream stream = new MemoryStream();
            await image.CopyToAsync(stream);
            stream.Position = 0;

            using var magickImage = new MagickImage(stream);

            // Determine output format: PNG if originally PNG, else JPEG
            MagickFormat outputFormat;
            string extension;
            if (image.ContentType == "image/png")
            {
                outputFormat = MagickFormat.Png;
                extension = ".png";
            }
            else
            {
                outputFormat = MagickFormat.Jpeg;
                extension = ".jpg";
            }

            // Set quality if saving as JPEG
            if (outputFormat == MagickFormat.Jpeg)
            {
                magickImage.Format = MagickFormat.Jpeg;
                magickImage.Quality = 90;
            }

            string fileName = Guid.NewGuid().ToString() + extension;
            string path = Path.Combine(location, fileName);

            await magickImage.WriteAsync(path);

            createdFiles.Add(path);

            return Path.Combine(folder, fileName);
        }
    }
}
