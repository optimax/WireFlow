using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        public string SelectedImage { get; set; }



        private WireFlowEngine Engine { get; }
        private ObservableCollection<string> images;

        public SelectImageViewModel(WireFlowEngine engine)
        {
            Engine = engine;
            // A project without images may have no list of them
            IEnumerable<string> projectImages = Engine?.Images;
            Images = new ObservableCollection<string>(projectImages ?? Array.Empty<string>());
        }


    }
}
