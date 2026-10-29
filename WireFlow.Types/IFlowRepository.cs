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
        IList<string> Pages { get; }
        IList<string> Parts { get; }
        IList<string> Images { get; }


        FlowProject NewProject(string docName);
        FlowProject OpenProject(string projectFileName);
        FlowPage GetMasterPage(string nodeName);


        FlowPage GetPage(string nodeName);
        FlowElementBase GetPart(string partName);
        byte[] LoadImage(string imageName);

        Action<string> ProjectFileLoaded { get; set; }


        void CreatePage(string nodeName, string layoutName);
        void DeletePage(string pageName);
        void SavePage(FlowPage page);
        void RenamePage(string oldName, string newName);

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

