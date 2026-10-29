using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WireFlow
{
    public class UIGroupProxy: UICanvasProxy
    {
        public static ImageSource _imageSource = RetrieveImageBitmap("group.png");

        public override ImageSource ImageSource => _imageSource;

        public override string ContentInfo => $"{Elements.Count} objects";

        public UIGroupProxy(FrameworkElement element): base(element)
        {
            Type = "Group";
        }
    }
}
