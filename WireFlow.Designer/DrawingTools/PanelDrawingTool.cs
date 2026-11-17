using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// Draws a panel. Elements drawn starting inside it become its children, which it places by their Alignment: along its
    /// sides, across the room left, or, with none, where they were drawn.
    /// </summary>
    public class PanelDrawingTool : InteractiveDrawingTool
    {
        public override string Title => "Panel";
        public override string Glyph => "panel";

        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenRectangleCursor"] as FrameworkElement)?.Cursor);

        // A faint gray, so that an empty panel shows on a light page and a dark one alike
        public override FrameworkElement CreateElement()
        {
            return new AlignPanel()
            {
                Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80)),
            };
        }

        private Cursor cursor = null;
    }
}
