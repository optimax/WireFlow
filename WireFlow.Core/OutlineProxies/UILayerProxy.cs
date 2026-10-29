using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UILayerProxy: UICanvasProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("layer.png");

        public override ImageSource ImageSource => _imageSource;

        public UILayerProxy(FrameworkElement element): base(element)
        {
            Type = "Layer";
        }
    }
}
