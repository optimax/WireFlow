using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WireFlow
{
    class SeparatorDrawingTool: Separator, IDrawingTool
    {
        public string Title => null;
        public MouseDragMode DesignerMode => MouseDragMode.None;

        public SeparatorDrawingTool()
        {
            BorderBrush = Brushes.CornflowerBlue;
            BorderThickness = new Thickness(1);
        }

        public FrameworkElement CreateElement()
        {
            return null;
        }
    }
}
