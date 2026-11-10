using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// The geometry of a tool glyph named by its SVG's file name ("rectangle", "layer"), from the application's resources
    /// (ToolGlyphs.xaml, "ToolGlyph.&lt;name&gt;"), as DrawingTool.GlyphGeometry finds a tool's; null for a name it has none of.
    /// </summary>
    public class ToolGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string name ? Application.Current?.TryFindResource("ToolGlyph." + name) as Geometry : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
