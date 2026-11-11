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


        IList<string> Layouts { get; }
        /// <summary>The keys of the pages (<see cref="FlowPage.Key"/>), in the project's order; the first is the entry point.</summary>
        IList<string> Pages { get; }
        IList<string> Parts { get; }
        IList<string> Images { get; }


        FlowProject NewProject(string docName);
        FlowProject OpenProject(string projectFileName);
        FlowPage GetMasterPage(string nodeName);


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

        void CreateLayout(string nodeName, string layoutName);
        void DeleteLayout(string pageName);
        void SaveLayout(FlowPage page);
        void RenameLayout(string oldName, string newName);

        void CreatePart(string partName);
        void DeletePart(string partName);
        void SavePart(FlowPart part);
        void RenamePart(string oldName, string newName);


        void SaveProject();


        ImportImageResult CanImportImage(string fileName);
        ImportImageResult ImportImage(string fileName);

        void ImportImages(IEnumerable<string> fileNames);
        void DeleteImages(IList fileNames);

    }
}

