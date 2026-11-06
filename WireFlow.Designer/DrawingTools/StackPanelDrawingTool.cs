using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WireFlow
{
    /// <summary>
    /// Draws a stack panel. Elements drawn starting inside it become its children, one after another in its Direction.
    /// </summary>
    public class StackPanelDrawingTool : InteractiveDrawingTool
    {
        public override string Title => "Stack Panel";
        public override string Glyph => "stack-panel";

        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenRectangleCursor"] as FrameworkElement)?.Cursor);

        // A faint gray, so that an empty panel shows on a light page and a dark one alike
        public override FrameworkElement CreateElement()
        {
            return new Stack()
            {
                Direction = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80)),
            };
        }

        private Cursor cursor = null;
    }
}
