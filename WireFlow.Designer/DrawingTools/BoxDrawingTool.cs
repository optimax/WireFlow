using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WireFlow
{
    public class BoxDrawingTool: InteractiveDrawingTool
    {
        public override string Title => "Box";
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
          cursor ?? (cursor = (Application.Current.Resources["PenBoxCursor"] as FrameworkElement)?.Cursor);

        public override FrameworkElement CreateElement()
        {
            var result = new Box()
            {
               FontSize = 16,
            };
            return result;
        }


        private Cursor cursor = null;
    }

}
