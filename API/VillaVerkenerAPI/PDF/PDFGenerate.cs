using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Diagnostics;

namespace VillaVerkenerAPI.PDF
{
    public class PDFGenerate
    {
        public void Main()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            PdfDocument document = new PdfDocument();

            PdfPage page = document.AddPage();
            Console.WriteLine("Addedpage");
            
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Arial", 20, XFontStyleEx.Regular);

            gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            document.Save(Path.Combine(Directory.GetCurrentDirectory(),"Images", "PDF","test.pdf"));
            Console.WriteLine("Document Saved");
        }
    }
}