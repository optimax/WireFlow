using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public class NavigateDrawingTool: ModeSelectionTool
    {
        public override string Title => "Navigate";
        public override MouseDragMode DesignerMode => MouseDragMode.Navigate;
        public override Cursor Cursor =>
          cursor ?? (cursor = (Application.Current.Resources["NavigateCursor"] as FrameworkElement)?.Cursor);

        private Cursor cursor = null;
    }
}
