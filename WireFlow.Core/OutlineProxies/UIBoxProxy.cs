using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIBoxProxy: UIElementProxy
    {
        private static ImageSource _imageSource = RetrieveImageBitmap("box.png");

        public override ImageSource ImageSource => _imageSource;

        public string Text => (UIElement as Box)?.Text;

        public override string ContentInfo => $"\"{Text}\"";

        public UIBoxProxy(FrameworkElement element): base(element)
        {
            Type = "Box";
        }

    }
}
