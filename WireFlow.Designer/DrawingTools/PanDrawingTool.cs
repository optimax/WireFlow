using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public class PanDrawingTool: ModeSelectionTool
    {
        public override string Title => "Pan";
        public override MouseDragMode DesignerMode => MouseDragMode.Pan;
        public override Cursor Cursor =>
           cursor ?? (cursor = (Application.Current.Resources["PanHandCursor"] as FrameworkElement)?.Cursor);

        private Cursor cursor = null;
    }
}
