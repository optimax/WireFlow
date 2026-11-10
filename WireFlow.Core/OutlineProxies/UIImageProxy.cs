using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIImageProxy: UIElementProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("image.png");

        public override ImageSource ImageSource => _imageSource;

        public override string Glyph => "image";

        public string ImageFileName => (UIElement as ImageBox)?.ImageFileName;

        public override string ContentInfo => ImageFileName;

        public UIImageProxy(FrameworkElement element): base(element)
        {
            Type = "Image";
        }

    }
}
