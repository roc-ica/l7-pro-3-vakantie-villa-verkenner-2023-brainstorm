using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Text;

namespace VillaVerkenerAPI.Services
{
    public struct Position
    {
        public double X;
        public double Y;
        public double Right => X + 1;
        public double Bottom => Y + 1;

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct ItemSize
    {
        public double Top;
        public double Left;
        public double Width;
        public double Height;

        public double Right => Left + Width;
        public double Bottom => Top + Height;

        public ItemSize(double top, double left, double width, double height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }
        public void MoveRight(double offset) => Left += offset;
        public void MoveDown(double offset) => Top += offset;
        public void MoveTo(double x, double y)
        {
            Left = x;
            Top = y;
        }

        public XRect ToXRect() => new XRect(Left, Top, Width, Height);
        public static ItemSize Combine(IEnumerable<ItemSize> items)
        {
            double top = double.MaxValue, left = double.MaxValue;
            double right = 0, bottom = 0;

            foreach (var item in items)
            {
                top = Math.Min(top, item.Top);
                left = Math.Min(left, item.Left);
                right = Math.Max(right, item.Right);
                bottom = Math.Max(bottom, item.Bottom);
            }

            return new ItemSize(top, left, right - left, bottom - top);
        }
        public static ItemSize GetStringBoxSize(IEnumerable<string> lines, XFont font)
        {
            double maxWidth = 0;
            double totalHeight = 0;

            foreach (var line in lines)
            {
                double lineWidth = line.Length * 5; // Approximate
                maxWidth = Math.Max(maxWidth, lineWidth);
                totalHeight += font.Height + 2;
            }

            return new ItemSize(0, 0, maxWidth + 2, totalHeight + 2);
        }
    }
    public class PageItem
    {
        public PdfPage Page { get; }
        public XGraphics Graphics { get; }
        public double Width => Page.Width.Point;
        public double Height => Page.Height.Point;

        public PageItem(PdfPage page)
        {
            Page = page;
            Graphics = XGraphics.FromPdfPage(page);
        }
    }
    public static class StringExtensions
    {
        public static string[] LimitLineLength(string text, int maxLength)
        {
            var lines = new List<string>();
            var builder = new StringBuilder();
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (builder.Length + word.Length > maxLength)
                {
                    lines.Add(builder.ToString().Trim());
                    builder.Clear();
                }
                builder.Append(word + " ");
            }

            if (builder.Length > 0)
                lines.Add(builder.ToString().Trim());

            return lines.ToArray();
        }
    }
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
                Fonts[name] = new XFont(fontName, size, style);
        }

        public XFont GetFont(string fontName)
        {
            if (Fonts.TryGetValue(fontName, out var font))
                return font;

            Console.WriteLine("Font not found, using default.");
            return new XFont("Arial", (int)BodyFontSize, XFontStyleEx.Regular);
        }
    }
}
