using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using WireFlow.Views;

namespace WireFlow
{
    public class ImageDrawingTool : InteractiveDrawingTool
    {
        public override string Title => "Image";
        public override MouseDragMode DesignerMode => MouseDragMode.Draw;

        public override Cursor Cursor =>
            cursor ?? (cursor = (Application.Current.Resources["PenImageCursor"] as FrameworkElement)?.Cursor);


        public override FrameworkElement CreateElement()
        {

            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Title = "Select an Image";
            //dlg.Filter = "All image files (png,jpg,gif,bmp)|*.png;*jpg;*jpeg;*.gif;*.bmp|All files|*";
            //var userResponse = dlg.ShowDialog();
            //if (userResponse != true)
            //    return null;




            var app = Application.Current as WireFlow.App;
            WireFlowEngine engine = app?.Engine;
            SelectImageViewModel vm = new SelectImageViewModel(engine);
            SelectImageDlg dlg = new SelectImageDlg(Application.Current.MainWindow, vm);
            var userResponse = dlg.ShowDialog();
            if (userResponse != true)
                return null;
            string selectedImage = vm.SelectedImage;
            if (string.IsNullOrWhiteSpace(selectedImage))
                return null;

            var imageBytes = engine.LoadImage(selectedImage);
            BitmapImage imageBits = ImageUtils.ImageFromBytes(imageBytes);

            //var imageBits = ImageUtils.RetrieveImageFromResource("WireFlow.Designer", "default-image.png");
            //BitmapImage imageBits = new BitmapImage(new Uri(imageFileName));


            var result = new ImageBox(imageBits)
            {
                Stretch = Stretch.Uniform,
                ImageFileName = selectedImage
            };
            return result;
        }

        private Cursor cursor = null;

    }
}
