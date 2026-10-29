using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WireFlow
{
    public class ImageBox: BaseDesignerObject
    {
        private Image image = new Image();

        private int pixelWidth;
        private int pixelHeight;

        public string ImageFileName { get; set; }

        public double SourceWidth => Source.Width;
        public double SourceHeight => Source.Height;

        public int PixelWidth => pixelWidth;
        public int PixelHeight => pixelHeight;


        public ImageSource Source
        {
            get { return image.Source; }
            set
            {
                image.Source = value;

                //sourceWidth = value?.Width ?? double.NaN;
                //sourceHeight = value?.Height ?? double.NaN;

                image.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));

                if (double.IsNaN(Width))
                    Width = image.DesiredSize.Width;
                if (double.IsNaN(Height))
                    Height = image.DesiredSize.Height;
            }
        }


        public Stretch Stretch
        {
            get { return image.Stretch; }
            set { image.Stretch = value; }
        }



        public ImageBox(ImageSource source)
        {
            var bits = source as BitmapImage;
            if (bits != null)
            {
                pixelWidth = bits.PixelWidth;
                pixelHeight = bits.PixelHeight;
            }
            Source = source;
            Content = image;
        }


        public override string ToString()
        {
            return $"{this.GetType().Name}:\"{ImageFileName}\", {(int)SourceWidth} x {(int)SourceHeight}";
        }
    }
}
