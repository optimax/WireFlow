using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public class SelectDrawingTool: ModeSelectionTool
    {
        public override string Title => "Select";
        public override MouseDragMode DesignerMode => MouseDragMode.Select;
        public override Cursor Cursor => Cursors.Arrow;
    }
}
