using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{
    public class TextDrawingTool : InteractiveDrawingTool
    {
        public override string Title => "Text";
        public override string Glyph => "text";
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenTextCursor"] as FrameworkElement)?.Cursor);

        public override FrameworkElement CreateElement()
        {
            // Transparent, so the text lies on what is under it, yet the whole frame can be clicked; centered in the frame
            var result = new WireFlowTextBlock()
            {
                Foreground = ColorUtils.StringToBrush("Black"),
                Background = Brushes.Transparent,
                TextAlignment = TextAlignment.Center,
                TextVerticalAlignment = VerticalAlignment.Center,
                Text = "Text Block",
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI"),
            };
            return result;
        }

        private Cursor cursor = null;
    }
}
