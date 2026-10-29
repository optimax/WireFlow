using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace WireFlow
{
    public class RectangleDrawingTool : InteractiveDrawingTool
    {
        public override string Title => "Rectangle";
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenRectangleCursor"] as FrameworkElement)?.Cursor);

        public override FrameworkElement CreateElement()
        {
            var result = new Rectangle()
            {
                Stroke = ColorUtils.StringToBrush("Black"),
                StrokeThickness = 3,
                Fill = ColorUtils.StringToBrush("#dedede"),
            };
            return result;
        }

        private Cursor cursor = null;
    }
}
