using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UILayoutProxy : UICanvasProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("layout.png");

        public override ImageSource ImageSource => _imageSource;

        public override string Glyph => "layout";

        public string NodeName => (UIElement as Layout)?.NodeName;

        public UILayoutProxy(FrameworkElement element): base(element)
        {
            Type = "Layout";
        }
    }
}
