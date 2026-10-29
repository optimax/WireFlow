using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WireFlow
{
    public class UIElementProxy
    {
        private static readonly ImageSource _imageSource = RetrieveImageBitmap("element.png");


        public FrameworkElement UIElement { get; set; }

        public string Type { get; set; }
        public string Name { get; set; }

        public double Left => Canvas.GetLeft(UIElement);
        public double Top => Canvas.GetTop(UIElement);
        public double Width => UIElement.Width;
        public double Height => UIElement.Height;

        public virtual ImageSource ImageSource => _imageSource;

        public virtual string ContentInfo => string.IsNullOrWhiteSpace(Name)? $"({AutoStr(Left)},{AutoStr(Top)},{AutoStr(Width)},{AutoStr(Height)})": Name;

        public override string ToString()
        {
            return $"{Type}: {Name}";
        }


        public UIElementProxy(FrameworkElement element)
        {
            UIElement = element;
            Type = element.GetType().Name;
            Name = element.Name;
        }


        public string AutoStr(double value)
        {
            if (double.IsNaN(value))
                return "Auto";
            else
                return value.ToString("N0");
        }


        protected static BitmapImage RetrieveImageBitmap(string imageName)
        {
            return ImageUtils.RetrieveImageFromResource("WireFlow.Core",imageName);
        }

    }
}
