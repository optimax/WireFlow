using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIPanelProxy : UICanvasProxy
    {
        public override ImageSource ImageSource => UIGroupProxy._imageSource;

        public override string Glyph => "panel";

        public override string ContentInfo => $"{(UIElement as AlignPanel)?.Children.Count} objects";

        public UIPanelProxy(FrameworkElement element) : base(element)
        {
            Type = "Panel";
        }
    }
}
