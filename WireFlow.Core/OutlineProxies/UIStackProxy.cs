using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIStackProxy : UICanvasProxy
    {
        public override ImageSource ImageSource => UIGroupProxy._imageSource;

        public override string ContentInfo => $"{(UIElement as Stack)?.Direction}, {(UIElement as Stack)?.Children.Count} objects";

        public UIStackProxy(FrameworkElement element) : base(element)
        {
            Type = "Stack Panel";
        }
    }
}
