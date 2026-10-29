using System;
using Xceed.Wpf.AvalonDock.Layout;

namespace WireFlow
{

    /// <summary>
    /// Holds a DesignerNavigator:USerControl
    /// </summary>
    public class DesignerNavigatorDocument: LayoutDocument
    {
        public DesignerNavigator Navigator { get; }


        public DesignerNavigatorDocument(DesignerNavigator navigator)
        {
            string contentId = Guid.NewGuid().ToString("N");
            ContentId = contentId;
            Navigator = navigator;
            Content = navigator;
        }


        public void TitleChangedHandler(DesignerNavigator sender, string newTitle)
        {
            Title = newTitle;
        }

    }
}
