using System;
using OptimaxTools.Dock.Layout;

namespace WireFlow
{

    /// <summary>
    /// Holds a DesignerNavigator:USerControl
    /// </summary>
    public class DesignerNavigatorDocument: LayoutDocument
    {
        public const string PageKind = "page";
        public const string LayoutKind = "layout";
        public const string PartKind = "part";

        public DesignerNavigator Navigator { get; }

        /// <summary>What the tab edits: PageKind, LayoutKind or PartKind; a page and a layout may have the same name.</summary>
        public string Kind { get; set; }


        public DesignerNavigatorDocument(DesignerNavigator navigator)
        {
            string contentId = Guid.NewGuid().ToString("N");
            ContentId = contentId;
            Navigator = navigator;
            Content = navigator;
            // The tab's mark of changes not saved (DocumentHeaderTemplate) follows the navigator
            navigator.ModifiedChanged += sender => RaisePropertyChanged(nameof(IsModified));
        }


        /// <summary>Whether its page, layout or part has changes not saved.</summary>
        public bool IsModified => Navigator.IsModified;


        public void TitleChangedHandler(DesignerNavigator sender, string newTitle)
        {
            Title = newTitle;
        }

    }
}
