using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace WireFlow
{
    public static class ImageUtils
    {
        public static BitmapImage RetrieveImageFromResource(string assemblyName, string imageName)
        {
            string uri = $"pack://application:,,,/{assemblyName};component/Images/{imageName}";
            BitmapImage result = new BitmapImage(new Uri(uri, UriKind.Absolute));
            return result;
        }


        public static BitmapImage ImageFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

    }
}
