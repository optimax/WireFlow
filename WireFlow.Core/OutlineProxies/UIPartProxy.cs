using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIPartProxy: UICanvasProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("part.png");

        public override ImageSource ImageSource => _imageSource;

        public string TemplateName => (UIElement as Part)?.TemplateName;

        public override string ContentInfo => TemplateName;


        public UIPartProxy(FrameworkElement element): base(element)
        {
            Type = "Part";
        }
    }
}
