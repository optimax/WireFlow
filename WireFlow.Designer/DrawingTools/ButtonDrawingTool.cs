using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WireFlow
{
    public class ButtonDrawingTool: DrawingTool
    {
        public override string Title => "Button";
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
          cursor ?? (cursor = (Application.Current.Resources["PenBoxCursor"] as FrameworkElement)?.Cursor);

        public override FrameworkElement CreateElement()
        {
            var result = new Button()
            {
                Content = "Submit"
            };
            return result;
        }


        private Cursor cursor = null;
    }
}
