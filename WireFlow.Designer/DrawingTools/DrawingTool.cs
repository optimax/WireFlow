using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WireFlow
{
    public abstract class DrawingTool: IDrawingTool
    {
        public abstract string Title { get; }
        public abstract MouseDragMode DesignerMode { get; }
        public abstract Cursor Cursor { get; }

        /// <summary>
        /// The name of the tool's glyph in the Tools list, null for none: Glyphs\&lt;name&gt;.svg, converted into
        /// Xaml\ToolGlyphs.xaml by Documents\make-tool-glyphs.cs.
        /// </summary>
        public virtual string Glyph => null;

        /// <summary>The glyph's outline on its 24 x 24 grid, from the application's resources.</summary>
        public Geometry GlyphGeometry => Glyph == null ? null : Application.Current?.TryFindResource("ToolGlyph." + Glyph) as Geometry;

        public virtual FrameworkElement CreateElement()
        {
            return null;
        }
    }
}
