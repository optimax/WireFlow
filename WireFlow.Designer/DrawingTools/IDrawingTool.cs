using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public interface IDrawingTool
    {
        string Title { get; }
        Cursor Cursor { get; }
        MouseDragMode DesignerMode { get; }

        FrameworkElement CreateElement();
    }

}
