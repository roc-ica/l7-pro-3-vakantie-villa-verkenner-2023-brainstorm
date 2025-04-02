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
            Console.WriteLine("RegisterProvider");
            PdfDocument document = new PdfDocument();

            PdfPage page = document.AddPage();
            Console.WriteLine("Addedpage");
            
            XGraphics gfx = XGraphics.FromPdfPage(page);
            Console.WriteLine("3");

            XFont font = new XFont("Arial", 20, XFontStyleEx.Regular);
            Console.WriteLine("4");

            gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            Console.WriteLine("5");
            document.Save("C:\\testPDF.pdf");
            Console.WriteLine("Document Saved");
        }
    }
}