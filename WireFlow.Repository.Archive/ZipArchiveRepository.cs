using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireFlow.Repository.Archive
{
    public class ZipArchiveRepository: IFlowRepository
    {
        public string DocumentFileFilters { get; }
        public string DefaultFileExt { get; }
        public string DefaultIndexNode { get; }
        public string ProjectFileName { get; }
        public IList<string> Layouts { get; }
        public IList<string> Pages { get; }
        public IList<string> Parts { get; }
        public IList<string> Images { get; }


        public FlowProject NewProject(string docName)
        {
            throw new NotImplementedException();
        }

        public FlowProject OpenProject(string projectFileName)
        {
            throw new NotImplementedException();
        }

        public FlowPage GetMasterPage(string nodeName)
        {
            throw new NotImplementedException();
        }

        public void CreatePage(string nodeName, string layoutName)
        {
            throw new NotImplementedException();
        }

        public void DeletePage(string pageName)
        {
            throw new NotImplementedException();
        }

        public FlowPage GetPage(string nodeName)
        {
            throw new NotImplementedException();
        }

        public FlowElementBase GetPart(string partName)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadImage(string imageName)
        {
            throw new NotImplementedException();
        }

        public Action<string> ProjectFileLoaded { get; set; }


        public void SavePage(FlowPage page)
        {
            throw new NotImplementedException();
        }

        public void RenameLayout(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public void CreatePart(string partName)
        {
            throw new NotImplementedException();
        }

        public void DeletePart(string partName)
        {
            throw new NotImplementedException();
        }

        public void SavePart(FlowPart part)
        {
            throw new NotImplementedException();
        }

        public void RenamePart(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public void SaveProject()
        {
            throw new NotImplementedException();
        }

        public void RenamePage(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public void CreateLayout(string nodeName, string layoutName)
        {
            throw new NotImplementedException();
        }

        public void DeleteLayout(string pageName)
        {
            throw new NotImplementedException();
        }

        public void SaveLayout(FlowPage page)
        {
            throw new NotImplementedException();
        }

        public ImportImageResult CanImportImage(string fileName)
        {
            throw new NotImplementedException();
        }

        ImportImageResult IFlowRepository.ImportImage(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ImportImages(IEnumerable<string> fileNames)
        {
            throw new NotImplementedException();
        }

        public void DeleteImages(IList fileNames)
        {
            throw new NotImplementedException();
        }
    }
}
