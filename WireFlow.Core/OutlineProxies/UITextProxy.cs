using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WireFlow
{
    public class UITextProxy: UIElementProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("text.png");

        public override ImageSource ImageSource => _imageSource;

        public string Text => (UIElement as TextBlock)?.Text;

        public override string ContentInfo => $"\"{Text}\"";

        public UITextProxy(FrameworkElement element): base(element)
        {
            Type = "Text";
        }

    }
}
