using System.Windows;
using System.Windows.Input;

namespace WireFlow
{
    public abstract class DrawingTool: IDrawingTool
    {
        public abstract string Title { get; }
        public abstract MouseDragMode DesignerMode { get; }
        public abstract Cursor Cursor { get; }

        public virtual FrameworkElement CreateElement()
        {
            return null;
        }
    }
}
