namespace VillaVerkenerAPI.Services
{
    public static class APIUrlHandler
    {
        //Helps generate URLs for images and PDFs in the API

        public readonly static string BaseUrl = "87.106.224.51:3012/";
        public readonly static string ImageUrl = $"{BaseUrl}Images/";
        public readonly static string PDFUrl = $"{BaseUrl}Images/PDF/";
        public static string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return "";
            }
            return $"{ImageUrl}{imagePath}";
        }

        public static string GetPDFUrl(string PdfFileName)
        {
            if (string.IsNullOrEmpty(PdfFileName))
            {
                return "";
            }
            return $"{PDFUrl}{PdfFileName}";
        }
    }
}
