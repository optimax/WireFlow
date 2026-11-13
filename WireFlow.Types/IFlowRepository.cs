using System;
using System.Collections;
using System.Collections.Generic;

namespace WireFlow
{
    public interface IFlowRepository
    {
        string DocumentFileFilters { get; }
        string DefaultFileExt { get; }
        string DefaultIndexNode { get; }
        string ProjectFileName { get; }


        /// <summary>The keys of the layouts (<see cref="FlowPage.Key"/>), by name.</summary>
        IList<string> Layouts { get; }
        /// <summary>The keys of the pages (<see cref="FlowPage.Key"/>), in the project's order; the first is the entry point.</summary>
        IList<string> Pages { get; }
        IList<string> Parts { get; }
        IList<string> Images { get; }


        FlowProject NewProject(string docName);
        FlowProject OpenProject(string projectFileName);
        /// <summary>The layout with this key, with its unsaved edits, as a page.</summary>
        FlowPage GetMasterPage(string layoutKey);
        /// <summary>The name of the layout with this key; null when there is none.</summary>
        string LayoutName(string layoutKey);
        /// <summary>The key of the layout with this name, ignoring case; null when there is none.</summary>
        string LayoutKey(string layoutName);


        FlowPage GetPage(string pageKey);
        /// <summary>The name of the page with this key; null when there is none.</summary>
        string PageName(string pageKey);
        /// <summary>The key of the page with this name, ignoring case; null when there is none.</summary>
        string PageKey(string pageName);
        FlowElementBase GetPart(string partName);
        byte[] LoadImage(string imageName);

        Action<string> ProjectFileLoaded { get; set; }


        /// <summary>Creates a page of this name, last in the order, and gives its key.</summary>
        string CreatePage(string pageName, string layoutName);
        void DeletePage(string pageKey);
        void SavePage(FlowPage page);
        void RenamePage(string pageKey, string newName);
        /// <summary>Puts the pages, named by key, in this order; the first is the project's entry point (<see cref="FlowProject.Home"/>).</summary>
        void SetPageOrder(IList<string> pageKeys);

        /// <summary>Creates a layout of this name, on the layout named, and gives its key.</summary>
        string CreateLayout(string layoutName, string masterName);
        void DeleteLayout(string layoutKey);
        void SaveLayout(FlowPage page);
        void RenameLayout(string layoutKey, string newName);

        void CreatePart(string partName);
        void DeletePart(string partName);
        void SavePart(FlowPart part);
        void RenamePart(string oldName, string newName);


        void SaveProject();
        // Copies the project, as it is in its file, to another file, which becomes the project's file
        void SaveProjectAs(string projectFileName);
        // Writes the project's own values (FlowProject) into its file at once, as SetPageOrder writes the order
        void SaveProjectSettings();


        ImportImageResult CanImportImage(string fileName);
        ImportImageResult ImportImage(string fileName);

        void ImportImages(IEnumerable<string> fileNames);
        void DeleteImages(IList fileNames);

    }
}

