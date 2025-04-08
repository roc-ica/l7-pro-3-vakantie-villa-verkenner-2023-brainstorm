using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        public void Main(Villa villa)
        {
            PageItem page = _pages[0];

            ItemSize header = AddCompanyInfo(page);
            ItemSize imageBlock = AddMainImage(villa, header.Bottom, page);
            ItemSize details = AddVillaDetails(villa, imageBlock, page);
            ItemSize description = AddDescription(villa, details, page);

            string fileName = $"flyer_{villa.Naam.Trim().Replace(" ", "_")}.pdf";
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "PDF", fileName);

            _document.Save(outputPath);
            Console.WriteLine($"Document saved: {outputPath}");
        }

        private ItemSize AddCompanyInfo(PageItem page)
        {
            XFont font = _layout.GetFont("Small");
            string[] lines = new[]
            {
                "Vakantie Villa Verkenner",
                "address line 1",
                "address line 2",
                "info@villaverkenner",
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

        private ItemSize AddMainImage(Villa villa, double startY, PageItem page)
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

            ItemSize imageBlock = new ItemSize(combinedTop.Bottom, _layout.MarginLeft, _layout.ImageMaxWidth, _layout.ImageMaxHeight);
            Image? image = villa.Images.FirstOrDefault(i => i.IsPrimary == 1);

            string imagePath = Path.Combine("Images", image?.ImageLocation ?? string.Empty);
            imagePath = Path.Combine("Images", "CharmingWasillanHome", "Exterior.png");

            if (image == null || !File.Exists(imagePath))
                return ItemSize.Combine(new[] { AddFailedImageBlock(imageBlock, page), combinedTop });

            XImage xImage = XImage.FromFile(imagePath);
            page.Graphics.DrawImage(xImage, imageBlock.ToXRect(), new XRect(0, 0, xImage.PixelWidth, xImage.PixelHeight), XGraphicsUnit.Point);

            return ItemSize.Combine(new[] { imageBlock, combinedTop });
        }

        private ItemSize AddVillaDetails(Villa villa, ItemSize imageBlock, PageItem page)
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
            textBlock.MoveTo(imageBlock.Right + _layout.MarginRight, imageBlock.Top);

            XRect rect = textBlock.ToXRect();
            foreach (string? line in lines)
            {
                page.Graphics.DrawString(line, font, XBrushes.Black, rect, XStringFormats.CenterLeft);
                rect.Y += font.Height + 2;
            }

            return ItemSize.Combine(new[] { imageBlock, textBlock });
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
            double y = placeholder.Top + _layout.MarginLeft;

            page.Graphics.DrawRectangle(XBrushes.Red, new XRect(x, y, placeholder.Width, placeholder.Height));
            page.Graphics.DrawLine(XPens.White, x, y, x + placeholder.Width, y + placeholder.Height);
            page.Graphics.DrawLine(XPens.White, x, y + placeholder.Height, x + placeholder.Width, y);

            page.Graphics.DrawString("No image found", _layout.GetFont("Body"), XBrushes.White, new XRect(x, y, placeholder.Width, placeholder.Height), XStringFormats.Center);

            return placeholder;
        }
    }
}
