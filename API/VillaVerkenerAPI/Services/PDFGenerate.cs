using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Services
{
    public class PDFGenerate
    {
        private readonly PdfDocument _document = new PdfDocument();
        private readonly List<PageItem> _pages = new List<PageItem>();
        private readonly PdfLayoutConfig _layout;

        public PDFGenerate()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _layout = new PdfLayoutConfig();
            _layout.AddFont("Title", "Arial", (int)_layout.HeaderFontSize, XFontStyleEx.Regular);
            _layout.AddFont("Body", "Arial", (int)_layout.BodyFontSize, XFontStyleEx.Regular);
            _layout.AddFont("Small", "Arial", (int)_layout.SmallFontSize, XFontStyleEx.Regular);

            _pages.Add(new PageItem(_document.AddPage()));
        }

        public RequestResponse Main(Villa villa, string outputPath, bool shouldRegenerate = false)
        {
            try
            {
                if (File.Exists(outputPath))
                {
                    if (shouldRegenerate)
                    {
                        File.Delete(outputPath);
                    }
                    else
                    {
                        Console.WriteLine($"Document already exists: {outputPath}");
                        return RequestResponse.Successfull("Document already exists", new Dictionary<string, string> { { "PDF", outputPath } });
                    }
                }

                GeneratePDF(villa);

                _document.Save(outputPath);
                _document.Close();
                Console.WriteLine($"Document saved: {outputPath}");
                return RequestResponse.Successfull("PDF generated successfully", new Dictionary<string, string> { { "PDF", outputPath } });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                return RequestResponse.Failed("PDF generation failed", new Dictionary<string, string> { { "Error", ex.Message } });
            }
        }

        private void GeneratePDF(Villa villa)
        {
            PageItem page = _pages[0];

            ItemSize header = AddCompanyInfo(page);
            ItemSize[] mainBlocks = AddMainImage(villa, header.Bottom, page);
            ItemSize imageBlock = mainBlocks[0];
            ItemSize titleBlock = mainBlocks[1];

            ItemSize detailStartData = new ItemSize(titleBlock.Top, titleBlock.Left, imageBlock.Width, titleBlock.Height);
            ItemSize details = AddVillaDetails(villa, detailStartData, page);
            ItemSize description = AddDescription(villa, imageBlock, page);
        }

        private ItemSize AddCompanyInfo(PageItem page)
        {
            XFont font = _layout.GetFont("Small");
            string[] lines = new[]
            {
                "Vakantie Villa Verkenner",
                "Disketteweg 2",
                "3815 AV Amersfoort",
                "info@villaverkenner.nl",
                "000-1122334"
            };

            ItemSize textBlock = ItemSize.GetStringBoxSize(lines, font);
            double rightAlignedX = page.Width - textBlock.Width;
            textBlock.MoveTo(rightAlignedX, _layout.MarginTop);

            XRect rect = textBlock.ToXRect();
            foreach (string line in lines)
            {
                page.Graphics.DrawString(line, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect.Y += font.Height + 2;
            }

            string logoPath = Path.Combine("Images", "General", "logo.png");
            ItemSize logoBlock = new ItemSize(textBlock.Top, _layout.MarginLeft, 100, textBlock.Height);

            if (File.Exists(logoPath))
            {
                XImage logo = XImage.FromFile(logoPath);
                page.Graphics.DrawImage(logo, logoBlock.ToXRect(), new XRect(0, 0, logo.PixelWidth, logo.PixelHeight), XGraphicsUnit.Point);
            }
            else
            {
                logoBlock = AddFailedImageBlock(logoBlock, page);
            }

            return ItemSize.Combine(new[] { textBlock, logoBlock });
        }

        private ItemSize[] AddMainImage(Villa villa, double startY, PageItem page)
        {
            XFont titleFont = _layout.GetFont("Title");
            XFont bodyFont = _layout.GetFont("Body");

            ItemSize titleBlock = ItemSize.GetStringBoxSize(new[] { villa.Naam }, titleFont);
            titleBlock.MoveTo(_layout.MarginLeft, startY + _layout.MarginTop);
            page.Graphics.DrawString(villa.Naam, titleFont, XBrushes.Black, titleBlock.ToXRect(), XStringFormats.TopLeft);

            ItemSize addressBlock = ItemSize.GetStringBoxSize(new[] { villa.Locatie }, bodyFont);
            addressBlock.MoveTo(_layout.MarginLeft, titleBlock.Bottom);
            page.Graphics.DrawString(villa.Locatie, bodyFont, XBrushes.Black, addressBlock.ToXRect(), XStringFormats.TopLeft);

            ItemSize combinedTop = ItemSize.Combine(new[] { titleBlock, addressBlock });

            ItemSize imageBlock = drawImage(villa, combinedTop, page);


            return new[] { imageBlock, combinedTop };
        }

        private ItemSize drawImage(Villa villa, ItemSize combinedTop, PageItem page)
        {
            ItemSize imageBlock = new ItemSize(combinedTop.Bottom, _layout.MarginLeft, _layout.ImageMaxWidth, _layout.ImageMaxHeight);
            Image? image = villa.Images.FirstOrDefault(i => i.IsPrimary == 1);

            string imagePath = Path.Combine("Images", image?.ImageLocation ?? string.Empty);

            if (image == null || !File.Exists(imagePath))
            {
                imageBlock = AddFailedImageBlock(imageBlock, page);
                return imageBlock;
            }

            XImage xImage = XImage.FromFile(imagePath);

            // 72 is DPI for PDF
            double imgWidth = xImage.PixelWidth * 72.0 / xImage.HorizontalResolution;
            double imgHeight = xImage.PixelHeight * 72.0 / xImage.VerticalResolution;

            double maxWidth = imageBlock.Width;
            double maxHeight = imageBlock.Height;

            double scale = Math.Min(maxWidth / imgWidth, maxHeight / imgHeight);

            double drawWidth = imgWidth * scale;
            double drawHeight = imgHeight * scale;

            double drawLeft = imageBlock.Left + (imageBlock.Width - drawWidth) / 2;
            double drawTop = imageBlock.Top + (imageBlock.Height - drawHeight) / 2;

            page.Graphics.DrawImage(xImage,
                new XRect(drawLeft, drawTop, drawWidth, drawHeight),
                new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight),
                XGraphicsUnit.Point);

            return imageBlock;
        }

        private ItemSize AddVillaDetails(Villa villa, ItemSize previousBlock, PageItem page)
        {
            XFont font = _layout.GetFont("Body");
            string[] lines = new[]
            {
                $"Prijs: {villa.Prijs} per nacht",
                $"Capaciteit: {villa.Capaciteit}",
                $"Slaapkamers: {villa.Slaapkamers}",
                $"Badkamers: {villa.Badkamers}"
            };

            ItemSize textBlock = ItemSize.GetStringBoxSize(lines, font);
            textBlock.MoveTo(previousBlock.Right + _layout.MarginRight, previousBlock.Top);
            textBlock.MoveDown(13);

            XRect rect = textBlock.ToXRect();
            foreach (string? line in lines)
            {
                page.Graphics.DrawString(line, font, XBrushes.Black, rect, XStringFormats.CenterLeft);
                rect.Y += font.Height + 2;
            }

            return ItemSize.Combine(new[] { previousBlock, textBlock });
        }

        private ItemSize AddDescription(Villa villa, ItemSize previousBlock, PageItem page)
        {
            string[] lines = StringExtensions.LimitLineLength(villa.Omschrijving, 70);
            XFont font = _layout.GetFont("Body");

            ItemSize block = ItemSize.GetStringBoxSize(lines, font);
            block.MoveTo(_layout.MarginLeft, previousBlock.Bottom + _layout.MarginBottom);

            XRect rect = block.ToXRect();
            foreach (string line in lines)
            {
                page.Graphics.DrawString(line, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect.Y += font.Height + 2;
            }

            return block;
        }

        private ItemSize AddFailedImageBlock(ItemSize placeholder, PageItem page)
        {
            Console.WriteLine("Rendering failed image block...");

            double x = _layout.MarginLeft;
            double y = placeholder.Top;

            page.Graphics.DrawRectangle(XBrushes.Red, new XRect(x, y, placeholder.Width, placeholder.Height));
            page.Graphics.DrawLine(XPens.White, x, y, x + placeholder.Width, y + placeholder.Height);
            page.Graphics.DrawLine(XPens.White, x, y + placeholder.Height, x + placeholder.Width, y);

            page.Graphics.DrawString("No image found", _layout.GetFont("Body"), XBrushes.White, new XRect(x, y, placeholder.Width, placeholder.Height), XStringFormats.Center);

            return placeholder;
        }
    }
}
