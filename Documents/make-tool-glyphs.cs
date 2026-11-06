#:property TargetFramework=net10.0-windows
#:property UseWPF=true
#:property PublishAot=false
#:property Nullable=disable
#:property RootNamespace=MakeToolGlyphs

// Converts the tool glyphs, WireFlow.Designer\Glyphs\*.svg (the authoritative source), into
// WireFlow.Designer\Xaml\ToolGlyphs.xaml: one Geometry per glyph, keyed "ToolGlyph.<file name>", and the glyphs' stroke
// width as "ToolGlyphStrokeThickness". The Designer uses the XAML only; it reads no SVG. Change an SVG, or add one, and run
// this again from the repository root rather than editing the XAML:
//   dotnet run Documents\make-tool-glyphs.cs
//
// The SVGs are line glyphs on a 24 x 24 grid, as PDFTools' toolbar icons: the root element sets viewBox "0 0 24 24",
// fill "none", stroke "currentColor", the stroke width (the same in all of them) and round caps and joins; the elements
// inside are path, line, polyline, polygon, rect (with rx), circle and ellipse, without attributes of their own beyond
// their geometry. Anything else stops the conversion.
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
string glyphs = Path.Combine(root, "WireFlow.Designer", "Glyphs");
string output = Path.Combine(root, "WireFlow.Designer", "Xaml", "ToolGlyphs.xaml");
XNamespace svg = "http://www.w3.org/2000/svg";
var invariant = CultureInfo.InvariantCulture;
string N(double value) => value.ToString("0.###", invariant);
double A(XElement element, string name) =>
    double.Parse((string)element.Attribute(name) ?? throw new InvalidDataException($"<{element.Name.LocalName}> has no {name}"), invariant);

// An element's outline in WPF's path markup, which SVG's path data is (with the separators the SVGs use)
string Markup(XElement element)
{
    string Points(bool close)
    {
        var numbers = ((string)element.Attribute("points")).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(n => double.Parse(n, invariant)).ToArray();
        var text = new StringBuilder($"M{N(numbers[0])},{N(numbers[1])}");
        for (int i = 2; i + 1 < numbers.Length; i += 2)
            text.Append($" L{N(numbers[i])},{N(numbers[i + 1])}");
        return close ? text.Append(" Z").ToString() : text.ToString();
    }
    string Ellipse(double cx, double cy, double rx, double ry) =>
        $"M{N(cx - rx)},{N(cy)} A{N(rx)},{N(ry)} 0 1 0 {N(cx + rx)},{N(cy)} A{N(rx)},{N(ry)} 0 1 0 {N(cx - rx)},{N(cy)} Z";
    switch (element.Name.LocalName)
    {
        case "path":
            return ((string)element.Attribute("d")).Trim();
        case "line":
            return $"M{N(A(element, "x1"))},{N(A(element, "y1"))} L{N(A(element, "x2"))},{N(A(element, "y2"))}";
        case "polyline":
            return Points(false);
        case "polygon":
            return Points(true);
        case "circle":
            return Ellipse(A(element, "cx"), A(element, "cy"), A(element, "r"), A(element, "r"));
        case "ellipse":
            return Ellipse(A(element, "cx"), A(element, "cy"), A(element, "rx"), A(element, "ry"));
        case "rect":
            double x = A(element, "x"), y = A(element, "y"), w = A(element, "width"), h = A(element, "height");
            double r = element.Attribute("rx") is null ? 0 : Math.Min(A(element, "rx"), Math.Min(w, h) / 2);
            if (r == 0)
                return $"M{N(x)},{N(y)} L{N(x + w)},{N(y)} L{N(x + w)},{N(y + h)} L{N(x)},{N(y + h)} Z";
            string corner(double toX, double toY) => $"A{N(r)},{N(r)} 0 0 1 {N(toX)},{N(toY)}";
            return $"M{N(x + r)},{N(y)} L{N(x + w - r)},{N(y)} {corner(x + w, y + r)} L{N(x + w)},{N(y + h - r)} {corner(x + w - r, y + h)} " +
                   $"L{N(x + r)},{N(y + h)} {corner(x, y + h - r)} L{N(x)},{N(y + r)} {corner(x + r, y)} Z";
        default:
            throw new InvalidDataException($"<{element.Name.LocalName}> is not an element the glyphs may use");
    }
}

string strokeWidth = null;
var entries = new StringBuilder();
foreach (string file in Directory.GetFiles(glyphs, "*.svg").OrderBy(f => f, StringComparer.Ordinal))
{
    string name = Path.GetFileNameWithoutExtension(file);
    try
    {
        var document = XDocument.Load(file);
        var top = document.Root;
        if (top.Name != svg + "svg" || (string)top.Attribute("viewBox") != "0 0 24 24" || (string)top.Attribute("fill") != "none" ||
            (string)top.Attribute("stroke") != "currentColor")
            throw new InvalidDataException("the root is not <svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\">");
        string width = (string)top.Attribute("stroke-width");
        if (strokeWidth != null && width != strokeWidth)
            throw new InvalidDataException($"its stroke width, {width}, is not the others', {strokeWidth}");
        strokeWidth = width;
        string comment = top.Nodes().OfType<XComment>().Select(c => c.Value.Trim()).FirstOrDefault() ?? name;
        string markup = string.Join(" ", top.Elements().Select(Markup));
        // As WPF reads it, within the grid
        var bounds = Geometry.Parse(markup).Bounds;
        if (bounds.Left < 0 || bounds.Top < 0 || bounds.Right > 24 || bounds.Bottom > 24)
            throw new InvalidDataException($"it reaches outside the 24 x 24 grid: {bounds}");
        entries.Append($"    <!-- {comment} -->\r\n    <Geometry x:Key=\"ToolGlyph.{name}\">{markup}</Geometry>\r\n");
        Console.WriteLine($"{name}: {bounds}");
    }
    catch (Exception ex) when (ex is InvalidDataException || ex is FormatException || ex is System.Xml.XmlException)
    {
        Console.Error.WriteLine($"{file}: {ex.Message}");
        return 1;
    }
}

string xaml =
    "<!-- The tool glyphs of the Tools list, generated by Documents\\make-tool-glyphs.cs from WireFlow.Designer\\Glyphs\\*.svg.\r\n" +
    "     The SVGs are the source: change them and run the script again rather than editing this file. -->\r\n" +
    "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\r\n" +
    "                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\r\n" +
    "                    xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\">\r\n" +
    "    <!-- The glyphs' stroke width, on their 24 x 24 grid -->\r\n" +
    $"    <sys:Double x:Key=\"ToolGlyphStrokeThickness\">{strokeWidth}</sys:Double>\r\n" +
    entries +
    "</ResourceDictionary>\r\n";
File.WriteAllText(output, xaml, new UTF8Encoding(true));
Console.WriteLine("Wrote " + output);
return 0;
