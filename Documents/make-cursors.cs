#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false
#:property Nullable=disable
#:property RootNamespace=MakeCursors

// Draws the cursors of the Pick Color tool, WireFlow.Designer\Cursors\pick-color.cur (an eyedropper) and
// paint-bucket.cur, from the tool glyphs of those names in WireFlow.Designer\Xaml\ToolGlyphs.xaml (made from the SVGs in
// WireFlow.Designer\Glyphs by make-tool-glyphs.cs). Change a glyph, or a place or a hot spot here, and run this again
// from the repository root rather than editing the .cur files:
//   dotnet run Documents\make-cursors.cs
// An optional second argument names a PNG to draw the cursors into, enlarged, to look at them.
//
// A cursor is 32 x 32 pixels with an alpha channel: the glyph, on its 24 x 24 grid, moved to its place, drawn in black
// over a white outline, so that it shows on a light page and a dark one. The hot spot is the point that clicks.
using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
string preview = args.Length > 1 ? args[1] : null;
ResourceDictionary glyphs;
using (var stream = File.OpenRead(Path.Combine(root, "WireFlow.Designer", "Xaml", "ToolGlyphs.xaml")))
    glyphs = (ResourceDictionary)XamlReader.Load(stream);
double stroke = (double)glyphs["ToolGlyphStrokeThickness"];
const int Size = 32;

// The glyph, where it lies in the cursor (its grid's top left corner), and the hot spot: the eyedropper's tip, and the
// bottom of the drop that falls from the bucket
var cursors = new[]
{
    (Name: "pick-color", Offset: new Vector(-1, 8), HotSpot: new Point(3, 28)),
    (Name: "paint-bucket", Offset: new Vector(1, 9), HotSpot: new Point(4, 29)),
};
var drawn = new System.Collections.Generic.List<BitmapSource>();
foreach (var cursor in cursors)
{
    var geometry = glyphs["ToolGlyph." + cursor.Name] as Geometry ?? throw new InvalidDataException($"ToolGlyphs.xaml has no ToolGlyph.{cursor.Name}");
    var visual = new DrawingVisual();
    using (var dc = visual.RenderOpen())
    {
        dc.PushTransform(new TranslateTransform(cursor.Offset.X, cursor.Offset.Y));
        Pen Line(Brush brush, double width) => new Pen(brush, width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        dc.DrawGeometry(null, Line(Brushes.White, stroke + 2.4), geometry);
        dc.DrawGeometry(Brushes.White, Line(Brushes.Black, stroke), geometry);
        dc.Pop();
    }
    var bitmap = new RenderTargetBitmap(Size, Size, 96, 96, PixelFormats.Pbgra32);
    bitmap.Render(visual);
    drawn.Add(bitmap);
    var pixels = new byte[Size * Size * 4];
    bitmap.CopyPixels(pixels, Size * 4, 0);
    // The file holds colors that are not multiplied by their alpha
    for (int i = 0; i < pixels.Length; i += 4)
    {
        int alpha = pixels[i + 3];
        if (alpha == 0 || alpha == 255)
            continue;
        for (int c = 0; c < 3; c++)
            pixels[i + c] = (byte)Math.Min(255, pixels[i + c] * 255 / alpha);
    }
    string path = Path.Combine(root, "WireFlow.Designer", "Cursors", cursor.Name + ".cur");
    using (var file = new BinaryWriter(File.Create(path)))
    {
        const int MaskRow = Size / 8, Header = 40;
        int image = Header + pixels.Length + Size * MaskRow;
        // The directory: one cursor, with its hot spot
        file.Write((ushort)0);
        file.Write((ushort)2);
        file.Write((ushort)1);
        file.Write((byte)Size);
        file.Write((byte)Size);
        file.Write((byte)0);
        file.Write((byte)0);
        file.Write((ushort)cursor.HotSpot.X);
        file.Write((ushort)cursor.HotSpot.Y);
        file.Write((uint)image);
        file.Write((uint)22);
        // The bitmap's header: twice as high, for the colors and the mask
        file.Write((uint)Header);
        file.Write(Size);
        file.Write(Size * 2);
        file.Write((ushort)1);
        file.Write((ushort)32);
        file.Write((uint)0);
        file.Write((uint)(pixels.Length + Size * MaskRow));
        file.Write(0);
        file.Write(0);
        file.Write((uint)0);
        file.Write((uint)0);
        // The colors, bottom row first, then the mask: a bit set where the cursor shows nothing
        for (int y = Size - 1; y >= 0; y--)
            file.Write(pixels, y * Size * 4, Size * 4);
        for (int y = Size - 1; y >= 0; y--)
            for (int b = 0; b < MaskRow; b++)
            {
                int bits = 0;
                for (int x = 0; x < 8; x++)
                    if (pixels[(y * Size + b * 8 + x) * 4 + 3] == 0)
                        bits |= 0x80 >> x;
                file.Write((byte)bits);
            }
    }
    // Windows reads it back
    using (var check = File.OpenRead(path))
        new System.Windows.Input.Cursor(check).Dispose();
    Console.WriteLine($"Wrote {path} (hot spot {cursor.HotSpot.X},{cursor.HotSpot.Y})");
}

if (preview != null)
{
    // Each cursor eight times as large on a light and a dark ground, with its hot spot marked
    const int Scale = 8;
    var sheet = new DrawingVisual();
    using (var dc = sheet.RenderOpen())
    {
        for (int ground = 0; ground < 2; ground++)
            for (int i = 0; i < drawn.Count; i++)
            {
                var place = new Rect(i * Size * Scale, ground * Size * Scale, Size * Scale, Size * Scale);
                dc.DrawRectangle(ground == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40)), null, place);
                dc.DrawImage(drawn[i], place);
                var spot = new Point(place.X + (cursors[i].HotSpot.X + 0.5) * Scale, place.Y + (cursors[i].HotSpot.Y + 0.5) * Scale);
                dc.DrawEllipse(null, new Pen(Brushes.Red, 1.5), spot, Scale * 0.75, Scale * 0.75);
            }
    }
    RenderOptions.SetBitmapScalingMode(sheet, BitmapScalingMode.NearestNeighbor);
    var image = new RenderTargetBitmap(drawn.Count * Size * Scale, 2 * Size * Scale, 96, 96, PixelFormats.Pbgra32);
    image.Render(sheet);
    var png = new PngBitmapEncoder();
    png.Frames.Add(BitmapFrame.Create(image));
    using (var file = File.Create(preview))
        png.Save(file);
    Console.WriteLine($"Wrote {preview}");
}
