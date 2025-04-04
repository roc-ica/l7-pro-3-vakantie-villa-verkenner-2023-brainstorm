using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.PDF
{
    public class PageItem
    {
        public PdfPage Page;
        public XGraphics Gfx;

        public PageItem(PdfPage page)
        {
            Page = page;
            Gfx = XGraphics.FromPdfPage(page);
        }
    }

    public class PDFGenerate
    {
        public Dictionary<string,XFont> fonts = new Dictionary<string, XFont>();
        private PdfDocument document = new PdfDocument();
        private List<PageItem> pages = new List<PageItem>();


        public PDFGenerate()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            AddFont("Title", "Arial", 20, XFontStyleEx.Regular);
            AddFont("Body", "Arial", 12, XFontStyleEx.Regular);
            AddFont("Small", "Arial", 10, XFontStyleEx.Regular);

            pages.Add(new PageItem(document.AddPage()));
        }

        public void Main(Villa villa)
        {
            AddCompanyInfo(pages[0]);

            AddMainImage(villa);

            document.Save(Path.Combine(Directory.GetCurrentDirectory(),"Images", "PDF",$"flyer_{villa.Naam.Trim().Replace(" ","_")}.pdf"));
            Console.WriteLine("Document Saved");
        }

        private void AddMainImage(Villa villa)
        {
            Image? image = villa.Images.FirstOrDefault(i => i.IsPrimary == 1);
            if (image == null)
            {
                Console.WriteLine("No image found");
                return;
            }
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", image.ImageLocation);
            imagePath = Path.Combine("Images","CharmingWasillanHome","Exterior.avif");
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Image not found: {imagePath}");
                return;
            }
            XImage xImage = XImage.FromFile(imagePath);
            XGraphics gfx = pages[0].Gfx;
            XFont font = GetFont("Title");
            double margin = 20;
            double x = margin;
            double y = margin;
            double width = pages[0].Page.Width - (2 * margin);
            double height = pages[0].Page.Height - (2 * margin) - font.Height - 10;
            gfx.DrawImage(xImage, new XRect(x, y, width, height),new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight), XGraphicsUnit.Point);
        }

        private void AddCompanyInfo(PageItem pageItem)
        {
            XGraphics gfx = pageItem.Gfx;
            XFont font = GetFont("Small");

            double margin = 20;

            double x = pageItem.Page.Width - margin;
            double y = margin;

            string[] lines = {
    "Vakantie Villa Verkenner",
    "address line 1",
    "address line 2",
    "info@villaverkenner",
    "000-1122334"
};

            foreach (string line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black, new XRect(margin, y, pageItem.Page.Width - (2 * margin), pageItem.Page.Height), XStringFormats.TopRight);
                y += font.Height + 2;
            }

        }

        private void AddFont(string name, string fontName, int size, XFontStyleEx style)
        {
            fonts ??= new Dictionary<string, XFont>();

            if (!fonts.ContainsKey(name))
            {
                fonts.Add(name, new XFont(fontName, size, style));
            }
            else
            {
                Console.WriteLine($"Font {name} already exists");
            }
        }

        private XFont GetFont(string FontName)
        {
            Console.WriteLine($"getting font {FontName} ");
            if (fonts.TryGetValue(FontName, out XFont? value))
            {
                return value;
            }
            else
            {
                Console.WriteLine("Font not found, creating new font");
                XFont font = new XFont(FontName, 20, XFontStyleEx.Regular);
                fonts.Add(FontName, font);
                return font;
            }
        }
    }
}