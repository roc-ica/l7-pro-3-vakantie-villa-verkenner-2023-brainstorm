using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;
using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.PDF
{
    public struct PdfLayoutConfig
    {
        public double MarginTop;
        public double MarginBottom;
        public double MarginLeft;
        public double MarginRight;
        public double ImageMaxWidth;
        public double ImageMaxHeight;
        public double HeaderFontSize;
        public double BodyFontSize;
        public double SmallFontSize;
        public Dictionary<string, XFont> Fonts;

        // Constructor to initialize default values
        public PdfLayoutConfig()
        {
            MarginTop = 20;
            MarginBottom = 20;
            MarginLeft = 20;
            MarginRight = 20;
            ImageMaxWidth = 400;
            ImageMaxHeight = 200;
            HeaderFontSize = 20;
            BodyFontSize = 12;
            SmallFontSize = 10;
            Fonts = new Dictionary<string, XFont>();
        }

        public void AddFont(string name, string fontName, int size, XFontStyleEx style)
        {
            if (!Fonts.ContainsKey(name))
            {
                Fonts.Add(name, new XFont(fontName, size, style));
            }
        }

        public XFont GetFont(string fontName)
        {
            if (Fonts.TryGetValue(fontName, out XFont? font))
            {
                return font;
            }
            else
            {
                Console.WriteLine("Font not found, creating new font");
                return new XFont("Arial", (int)BodyFontSize, XFontStyleEx.Regular);
            }
        }
    }

    public class PageItem
    {
        public PdfPage Page;
        public XGraphics Gfx;
        public double Width { get => Page.Width.Point; }
        public double Height { get => Page.Height.Point; }

        public PageItem(PdfPage page)
        {
            Page = page;
            Gfx = XGraphics.FromPdfPage(page);
        }
    }

    public struct ItemSize
    {
        public double top;
        public double left;
        public double width;
        public double height;
        public double right { get => left + width; }
        public double bottom { get => top + height; }
        public ItemSize(double top, double left, double width, double height)
        {
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
        }
    }

    public struct Position
    {
        public double x;
        public double y;
        public double right { get => x + 1; }
        public double bottom { get => y + 1; }
        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static double TextSize(string text)
        {
            double width = 0;
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                width = Math.Max(width, line.Length);
            }
            return width;
        }
    }

    public class PDFGenerate
    {
        private PdfDocument document = new PdfDocument();
        private List<PageItem> pages = new List<PageItem>();
        private PdfLayoutConfig layoutConfig;

        public PDFGenerate()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            layoutConfig = new PdfLayoutConfig();
            layoutConfig.AddFont("Title", "Arial", (int)layoutConfig.HeaderFontSize, XFontStyleEx.Regular);
            layoutConfig.AddFont("Body", "Arial", (int)layoutConfig.BodyFontSize, XFontStyleEx.Regular);
            layoutConfig.AddFont("Small", "Arial", (int)layoutConfig.SmallFontSize, XFontStyleEx.Regular);

            pages.Add(new PageItem(document.AddPage()));
        }

        public void Main(Villa villa)
        {

            ItemSize header = AddCompanyInfo(pages[0]);

            ItemSize image = AddMainImage(villa, header.bottom, pages[0]);

            document.Save(Path.Combine(Directory.GetCurrentDirectory(), "Images", "PDF", $"flyer_{villa.Naam.Trim().Replace(" ", "_")}.pdf"));
            Console.WriteLine("Document Saved");
        }

        private ItemSize AddFailedImageBlock(double start, PageItem pageItem)
        {
            Console.WriteLine("No image found");
            ItemSize imageSize = new ItemSize(start, layoutConfig.MarginLeft, 0, 0);
            XGraphics gfx = pageItem.Gfx;

            double margin = layoutConfig.MarginLeft;
            double x = margin;
            double y = start + margin;
            double width = layoutConfig.ImageMaxWidth;
            double height = layoutConfig.ImageMaxHeight;

            gfx.DrawRectangle(XBrushes.Red, new XRect(x, y, width, height));
            gfx.DrawLine(XPens.White, x, y, x + width, y + height);
            gfx.DrawLine(XPens.White, x, y + height, x + width, y);
            gfx.DrawString("No image found", layoutConfig.GetFont("Body"), XBrushes.White, new XRect(x, y, width, height), XStringFormats.Center);
            y += height + margin;
            imageSize.height = height;
            imageSize.width = width;
            return imageSize;
        }

        private ItemSize AddMainImage(Villa villa, double start, PageItem pageItem)
        {
            ItemSize imageSize = new ItemSize(start, layoutConfig.MarginLeft, 0, 0);
            Image? image = villa.Images.FirstOrDefault(i => i.IsPrimary == 1);

            if (image == null)
            {
                Console.WriteLine("No image found");
                return AddFailedImageBlock(start,pageItem);
            }

            string imagePath = Path.Combine("Images", "CharmingWasillanHome", "Exterior.png");

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Image not found: {imagePath}");
                return AddFailedImageBlock(start, pageItem);
            }

            XImage xImage = XImage.FromFile(imagePath);
            XGraphics gfx = pageItem.Gfx;

            double margin = layoutConfig.MarginLeft;
            double x = margin;
            double y = start + margin;
            double width = layoutConfig.ImageMaxWidth;
            double height = layoutConfig.ImageMaxHeight;

            gfx.DrawImage(xImage, new XRect(x, y, width, height), new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight), XGraphicsUnit.Point);

            y += height + margin;
            imageSize.height = height;
            imageSize.width = width;

            return imageSize;
        }

        private ItemSize AddCompanyInfo(PageItem pageItem)
        {
            XGraphics gfx = pageItem.Gfx;
            XFont font = layoutConfig.GetFont("Small");

            double margin = layoutConfig.MarginLeft;
            double top = layoutConfig.MarginTop;
            double left = pageItem.Page.Width.Point - margin;

            double x = left;
            double y = top;

            string[] lines = {
            "Vakantie Villa Verkenner",
            "address line 1",
            "address line 2",
            "info@villaverkenner",
            "000-1122334"
            };  

            foreach (string line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black, new XRect(margin, y, pageItem.Page.Width.Point - (2 * margin), pageItem.Page.Height.Point), XStringFormats.TopRight);
                y += font.Height + 2;
            }
            
            return new ItemSize(top,left,x, y);
        }
    }
}
