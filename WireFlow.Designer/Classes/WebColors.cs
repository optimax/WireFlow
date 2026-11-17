using System.Collections.Generic;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// The named colors of the Web: the 148 color keywords of CSS (CSS Color Module Level 4), in the order of their names,
    /// written as CSS writes them. They are WPF's named colors (<see cref="Colors"/>), with the same values, and besides
    /// them the seven spellings with "grey" and rebeccapurple; CSS's transparent, which is no color, is left out.
    /// </summary>
    public static class WebColors
    {
        /// <summary>The colors by name, in the order of their names.</summary>
        public static IReadOnlyList<KeyValuePair<string, Color>> Named { get; } = new[]
        {
            C("aliceblue", 0xF0F8FF), C("antiquewhite", 0xFAEBD7), C("aqua", 0x00FFFF), C("aquamarine", 0x7FFFD4),
            C("azure", 0xF0FFFF), C("beige", 0xF5F5DC), C("bisque", 0xFFE4C4), C("black", 0x000000),
            C("blanchedalmond", 0xFFEBCD), C("blue", 0x0000FF), C("blueviolet", 0x8A2BE2), C("brown", 0xA52A2A),
            C("burlywood", 0xDEB887), C("cadetblue", 0x5F9EA0), C("chartreuse", 0x7FFF00), C("chocolate", 0xD2691E),
            C("coral", 0xFF7F50), C("cornflowerblue", 0x6495ED), C("cornsilk", 0xFFF8DC), C("crimson", 0xDC143C),
            C("cyan", 0x00FFFF), C("darkblue", 0x00008B), C("darkcyan", 0x008B8B), C("darkgoldenrod", 0xB8860B),
            C("darkgray", 0xA9A9A9), C("darkgreen", 0x006400), C("darkgrey", 0xA9A9A9), C("darkkhaki", 0xBDB76B),
            C("darkmagenta", 0x8B008B), C("darkolivegreen", 0x556B2F), C("darkorange", 0xFF8C00), C("darkorchid", 0x9932CC),
            C("darkred", 0x8B0000), C("darksalmon", 0xE9967A), C("darkseagreen", 0x8FBC8F), C("darkslateblue", 0x483D8B),
            C("darkslategray", 0x2F4F4F), C("darkslategrey", 0x2F4F4F), C("darkturquoise", 0x00CED1), C("darkviolet", 0x9400D3),
            C("deeppink", 0xFF1493), C("deepskyblue", 0x00BFFF), C("dimgray", 0x696969), C("dimgrey", 0x696969),
            C("dodgerblue", 0x1E90FF), C("firebrick", 0xB22222), C("floralwhite", 0xFFFAF0), C("forestgreen", 0x228B22),
            C("fuchsia", 0xFF00FF), C("gainsboro", 0xDCDCDC), C("ghostwhite", 0xF8F8FF), C("gold", 0xFFD700),
            C("goldenrod", 0xDAA520), C("gray", 0x808080), C("green", 0x008000), C("greenyellow", 0xADFF2F),
            C("grey", 0x808080), C("honeydew", 0xF0FFF0), C("hotpink", 0xFF69B4), C("indianred", 0xCD5C5C),
            C("indigo", 0x4B0082), C("ivory", 0xFFFFF0), C("khaki", 0xF0E68C), C("lavender", 0xE6E6FA),
            C("lavenderblush", 0xFFF0F5), C("lawngreen", 0x7CFC00), C("lemonchiffon", 0xFFFACD), C("lightblue", 0xADD8E6),
            C("lightcoral", 0xF08080), C("lightcyan", 0xE0FFFF), C("lightgoldenrodyellow", 0xFAFAD2), C("lightgray", 0xD3D3D3),
            C("lightgreen", 0x90EE90), C("lightgrey", 0xD3D3D3), C("lightpink", 0xFFB6C1), C("lightsalmon", 0xFFA07A),
            C("lightseagreen", 0x20B2AA), C("lightskyblue", 0x87CEFA), C("lightslategray", 0x778899), C("lightslategrey", 0x778899),
            C("lightsteelblue", 0xB0C4DE), C("lightyellow", 0xFFFFE0), C("lime", 0x00FF00), C("limegreen", 0x32CD32),
            C("linen", 0xFAF0E6), C("magenta", 0xFF00FF), C("maroon", 0x800000), C("mediumaquamarine", 0x66CDAA),
            C("mediumblue", 0x0000CD), C("mediumorchid", 0xBA55D3), C("mediumpurple", 0x9370DB), C("mediumseagreen", 0x3CB371),
            C("mediumslateblue", 0x7B68EE), C("mediumspringgreen", 0x00FA9A), C("mediumturquoise", 0x48D1CC), C("mediumvioletred", 0xC71585),
            C("midnightblue", 0x191970), C("mintcream", 0xF5FFFA), C("mistyrose", 0xFFE4E1), C("moccasin", 0xFFE4B5),
            C("navajowhite", 0xFFDEAD), C("navy", 0x000080), C("oldlace", 0xFDF5E6), C("olive", 0x808000),
            C("olivedrab", 0x6B8E23), C("orange", 0xFFA500), C("orangered", 0xFF4500), C("orchid", 0xDA70D6),
            C("palegoldenrod", 0xEEE8AA), C("palegreen", 0x98FB98), C("paleturquoise", 0xAFEEEE), C("palevioletred", 0xDB7093),
            C("papayawhip", 0xFFEFD5), C("peachpuff", 0xFFDAB9), C("peru", 0xCD853F), C("pink", 0xFFC0CB),
            C("plum", 0xDDA0DD), C("powderblue", 0xB0E0E6), C("purple", 0x800080), C("rebeccapurple", 0x663399),
            C("red", 0xFF0000), C("rosybrown", 0xBC8F8F), C("royalblue", 0x4169E1), C("saddlebrown", 0x8B4513),
            C("salmon", 0xFA8072), C("sandybrown", 0xF4A460), C("seagreen", 0x2E8B57), C("seashell", 0xFFF5EE),
            C("sienna", 0xA0522D), C("silver", 0xC0C0C0), C("skyblue", 0x87CEEB), C("slateblue", 0x6A5ACD),
            C("slategray", 0x708090), C("slategrey", 0x708090), C("snow", 0xFFFAFA), C("springgreen", 0x00FF7F),
            C("steelblue", 0x4682B4), C("tan", 0xD2B48C), C("teal", 0x008080), C("thistle", 0xD8BFD8),
            C("tomato", 0xFF6347), C("turquoise", 0x40E0D0), C("violet", 0xEE82EE), C("wheat", 0xF5DEB3),
            C("white", 0xFFFFFF), C("whitesmoke", 0xF5F5F5), C("yellow", 0xFFFF00), C("yellowgreen", 0x9ACD32),
        };


        // A named color, of its RRGGBB
        private static KeyValuePair<string, Color> C(string name, int rgb) =>
            new KeyValuePair<string, Color>(name, Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb));
    }
}
