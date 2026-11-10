using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Optimax;

namespace WireFlow
{
    public class SelectImageViewModel: BaseDialogViewModel
    {
        public ObservableCollection<string> Images
        {
            get { return images; }
            set
            {
                if (images == value)
                    return;
                images = value;
                OnPropertyChanged();
            }
        }

        public string SelectedImage
        {
            get { return selectedImage; }
            set
            {
                if (selectedImage == value)
                    return;
                selectedImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Imports image files into the project, as the Images pane's + does, and selects the first one.</summary>
        public ICommand ImportImageCmd { get; }

        /// <summary>Deletes the selected image from the project, as the Images pane's − does.</summary>
        public ICommand DeleteImageCmd { get; }

        /// <summary>Whether the project's images changed while the dialog was open, so that the Images pane is brought up to date.</summary>
        public bool ImagesChanged { get; private set; }



        private WireFlowEngine Engine { get; }
        private ObservableCollection<string> images;
        private string selectedImage;

        public SelectImageViewModel(WireFlowEngine engine)
        {
            Engine = engine;
            LoadImages();
            ImportImageCmd = new RelayCommand(o => ImportImages(), o => Engine != null);
            DeleteImageCmd = new RelayCommand(o => DeleteImage(), o => Engine != null && !string.IsNullOrEmpty(SelectedImage));
        }


        // A project without images may have no list of them
        private void LoadImages()
        {
            IEnumerable<string> projectImages = Engine?.Images;
            Images = new ObservableCollection<string>(projectImages ?? Array.Empty<string>());
        }


        private void ImportImages()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Import an Image",
                Filter = "All image files (png,jpg,gif,bmp)|*.png;*jpg;*jpeg;*.gif;*.bmp|All files|*",
                Multiselect = true
            };
            if (dlg.ShowDialog() != true)
                return;
            Engine.ImportImages(dlg.FileNames);
            ImagesChanged = true;
            LoadImages();
            // An image is named by its file name; one already there under that name is not imported again, but is selected
            string first = Path.GetFileName(dlg.FileNames.FirstOrDefault() ?? "");
            SelectedImage = Images.FirstOrDefault(name => string.Equals(name, first, StringComparison.OrdinalIgnoreCase)) ?? SelectedImage;
        }


        private void DeleteImage()
        {
            string name = SelectedImage;
            var result = MessageBox.Show($"Delete the image \"{name}\" from the project?", "Delete Image", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            Engine.DeleteImages(new List<string> { name });
            ImagesChanged = true;
            SelectedImage = null;
            LoadImages();
        }
    }
}
