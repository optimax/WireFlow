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
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenTextCursor"] as FrameworkElement)?.Cursor);

        public override FrameworkElement CreateElement()
        {
            var result = new WireFlowTextBlock()
            {
                Foreground = ColorUtils.StringToBrush("Black"),
                Background = ColorUtils.StringToBrush("#dddddd"),
                Text = "Text Block",
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI"),
            };
            return result;
        }

        private Cursor cursor = null;
    }
}
