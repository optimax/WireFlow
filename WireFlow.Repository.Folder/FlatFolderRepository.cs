using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WireFlow
{
    public class FlatFolderRepository : IFlowRepository
    {
        #region Constants

        public const string IMAGES_FOLDER = "images";
        public const string PARTS_FOLDER = "parts";
        public const string LAYOUTS_FOLDER = "layouts";
        public const string PAGES_FOLDER = "pages";
        public const string PAGES_ARCHIVE_FOLDER = "pages.archive";
        public const string LAYOUTS_ARCHIVE_FOLDER = "layouts.archive";
        public const string PARTS_ARCHIVE_FOLDER = "parts.archive";
        public const string PROJECT_ARCHIVE_FOLDER = "project.archive";



        public const string PROJECT_EXTENSION = ".project";
        public const string MASTER_EXTENSION = ".master";
        public const string PAGE_EXTENSION = ".page";
        public const string PART_EXTENSION = ".part";

        public const string NEW_EXTENSION = ".new";


        #endregion


        #region Fields

        private string projectFileName;


        #endregion



        #region Properties

        public string DocumentFileFilters { get; set; } = "WireFlow Project|*.project|All files|*";
        public string DefaultFileExt { get; set; } = "wireflow";
        public string DefaultIndexNode { get; set; } = "index";
        public string DefaultBackground { get; set; } = "#333";


        public string ProjectFolderName { get; set; }

        public string ProjectFileName
        {
            get
            {
                return projectFileName;
            }
            set
            {
                if (projectFileName == value)
                    return;
                projectFileName = value;
            }
        }

        private FlowProject Project { get; set; }


        public IList<string> Pages => GetAllPages(ProjectFolderName);
        public IList<string> Layouts => GetAllLayouts(ProjectFolderName);
        public IList<string> Parts => GetAllParts(ProjectFolderName);
        public IList<string> Images => GetAllImages(ProjectFolderName);





        public IDictionary<string, string> MasterPages { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Events

        public Action<string> ProjectFileLoaded { get; set; }

        #endregion


        #region Static Utilities

        public static string SerializeToJSON(object obj)
        {
            try
            {
                string s = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto
                });
                return s;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in SerializeToJSON {ex.GetType()}\n{ex.Message}\nSTACK TRACE:\n{ex.StackTrace}\nSOURCE Object: {obj.GetType()}");
                return null;
            }
        }


        public static T DeserializeFromJSON<T>(string s, string entityName)
        {
            try
            {
                T result = JsonConvert.DeserializeObject<T>(s, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                return result;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION in DeserializeFromJSON<{typeof(T)}>({entityName}) {ex.GetType()}\n{ex.Message}\nSTACK TRACE:\n{ex.StackTrace}\nSOURCE STRING:\n{s}");
                return default(T);
            }
        }



        #endregion



        #region IWireFlowRepository


        public FlowProject NewProject(string docName)
        {
            if (string.IsNullOrWhiteSpace(docName))
                return null;
            ProjectFileName = null;
            string s;

            string projectFolder = Path.GetDirectoryName(docName);
            if (String.IsNullOrWhiteSpace(projectFolder))
                projectFolder = Path.GetDirectoryName(ProjectFileName);
            ProjectFolderName = projectFolder;

            string fileNameOnly = Path.GetFileName(docName);
            docName = Path.Combine(ProjectFolderName, fileNameOnly);
            ProjectFileName = docName;

            FlowProject result = new FlowProject()
            {
                Home = DefaultIndexNode,
                Background = DefaultBackground
            };
            s = SerializeToJSON(result);
            File.WriteAllText(docName, s);

            MasterPages.Clear();

            string masterFolder = Path.Combine(projectFolder, LAYOUTS_FOLDER);
            Directory.CreateDirectory(masterFolder);

            string pagesFolder = Path.Combine(projectFolder, PAGES_FOLDER);
            Directory.CreateDirectory(pagesFolder);

            string partsFolder = Path.Combine(projectFolder, PARTS_FOLDER);
            Directory.CreateDirectory(partsFolder);

            string imagesFolder = Path.Combine(projectFolder, IMAGES_FOLDER);
            Directory.CreateDirectory(imagesFolder);


            // Default page
            CreatePage(DefaultIndexNode, null);

            Project = result;

            return result;
        }



        public FlowProject OpenProject(string projectFileName)
        {
            string folderName = Path.GetDirectoryName(projectFileName);
            if (String.IsNullOrWhiteSpace(folderName))
                folderName = Path.GetDirectoryName(ProjectFileName);
            ProjectFolderName = folderName;

            LoadMasterPages(ProjectFolderName);

            string fileNameOnly = Path.GetFileName(projectFileName);
            projectFileName = Path.Combine(ProjectFolderName, fileNameOnly);
            if (!File.Exists(projectFileName))
                return null;
            string s = File.ReadAllText(projectFileName);
            var project = DeserializeFromJSON<FlowProject>(s, projectFileName);

            ProjectFileName = projectFileName;
            ProjectFileLoaded?.Invoke(this.projectFileName);

            Project = project;
            return project;
        }



        public FlowPage GetMasterPage(string nodeName)
        {
            if (String.IsNullOrWhiteSpace(nodeName))
                return null;
            string masterFileName;
            MasterPages.TryGetValue(nodeName, out masterFileName);
            if (String.IsNullOrWhiteSpace(masterFileName))
                return null;
            if (!File.Exists(masterFileName))
                return null;
            string s = File.ReadAllText(masterFileName);
            var master = DeserializeFromJSON<FlowPage>(s, nodeName + "|" + masterFileName);
            master.Name = nodeName;
            return master;
        }


        public FlowPage GetPage(string nodeName)
        {
            var fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, nodeName + NEW_EXTENSION);
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, nodeName + PAGE_EXTENSION);
                if (!File.Exists(fileName))
                    return null;
            }
            string s = File.ReadAllText(fileName);
            FlowPage pageNode = DeserializeFromJSON<FlowPage>(s, nodeName + "|" + fileName);
            if (pageNode == null)
                return null;
            pageNode.Name = nodeName;
            return pageNode;
        }


        public void DeletePage(string pageName)
        {
            string fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, pageName + NEW_EXTENSION);
            if (File.Exists(fileName))
                File.Delete(fileName);
            fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, pageName + PAGE_EXTENSION);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }




        public void CreatePage(string pageName, string layoutName)
        {
            FlowPage page = new FlowPage()
            {
                Name = pageName,
                Master = layoutName,
            };

            FlowLayer userLayer = new FlowLayer()
            {
                Name = "Layer1",
            };
            page.Layers.Add(userLayer);
            SavePage(page);
        }



        public void SavePage(FlowPage page)
        {
            Debug.Assert(page != null);
            string nodeName = page.Name;
            var s = SerializeToJSON(page);
            var fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, nodeName + NEW_EXTENSION);
            //Debug.WriteLine($"SAVING to {fileName}:\n{s}");
            File.WriteAllText(fileName, s);
        }



        public void RenamePage(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                return;
            if (string.IsNullOrWhiteSpace(newName))
                return;
            string fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, oldName + NEW_EXTENSION);
            string newFileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, newName + NEW_EXTENSION);
            if (File.Exists(fileName))
                File.Move(fileName, newFileName);
            fileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, oldName + PAGE_EXTENSION);
            newFileName = Path.Combine(ProjectFolderName, PAGES_FOLDER, newName + PAGE_EXTENSION);
            if (File.Exists(fileName))
                File.Move(fileName, newFileName);
        }



        public void CreateLayout(string nodeName, string layoutName)
        {
            FlowPage page = new FlowPage()
            {
                Name = nodeName,
                Master = layoutName,
            };

            FlowLayer userLayer = new FlowLayer()
            {
                Name = "Master1",
            };
            page.Layers.Add(userLayer);
            SaveLayout(page);
        }



        public void DeleteLayout(string pageName)
        {
            string fileName = Path.Combine(ProjectFolderName, LAYOUTS_FOLDER, pageName + NEW_EXTENSION);
            if (File.Exists(fileName))
                File.Delete(fileName);
            fileName = Path.Combine(ProjectFolderName, LAYOUTS_FOLDER, pageName + MASTER_EXTENSION);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }



        public void SaveLayout(FlowPage page)
        {
            Debug.Assert(page != null);
            string nodeName = page.Name;
            var s = SerializeToJSON(page);
            var fileName = Path.Combine(ProjectFolderName, LAYOUTS_FOLDER, nodeName + NEW_EXTENSION);
            File.WriteAllText(fileName, s);
        }



        public void RenameLayout(string oldName, string newName)
        {
            throw new NotImplementedException();
        }




        public void CreatePart(string partName)
        {
            FlowPart part = new FlowPart()
            {
                Name = partName,
            };
            SavePart(part);
        }


        public void DeletePart(string partName)
        {
            throw new NotImplementedException();
        }

        public void SavePart(FlowPart part)
        {
            Debug.Assert(part != null);
            string nodeName = part.Name;
            var s = SerializeToJSON(part);
            var fileName = Path.Combine(ProjectFolderName, PARTS_FOLDER, nodeName + NEW_EXTENSION);
            File.WriteAllText(fileName, s);
        }

        public void RenamePart(string oldName, string newName)
        {
            throw new NotImplementedException();
        }



        public void SaveProject()
        {
            string fileName;
            string s;


            // ### Phase 1: Save project file as .NEW
            Project.RevisionNo++;

            fileName = ProjectFileName;
            fileName = Path.ChangeExtension(fileName, NEW_EXTENSION);
            s = SerializeToJSON(Project);
            File.WriteAllText(fileName, s);


            // ### Phase 2: Archive existing revision & commit any .NEW entities

            // ### Project
            // Ensure archive folder is there
            string projArchiveFolder = Path.Combine(ProjectFolderName, PROJECT_ARCHIVE_FOLDER);
            Directory.CreateDirectory(projArchiveFolder);

            string archiveKey = "." + Project.RevisionNo;

            var projectNameOnly = Path.GetFileNameWithoutExtension(ProjectFileName) ?? "project";
            string archiveFileName = Path.Combine(projArchiveFolder, projectNameOnly + archiveKey);
            if (File.Exists(ProjectFileName))
                File.Move(ProjectFileName, archiveFileName);
            File.Move(fileName, ProjectFileName);



            // ### Layouts
            var cachedLayouts = CollectCachedLayouts();
            SaveCachedEntities(Project.RevisionNo, LAYOUTS_FOLDER, LAYOUTS_ARCHIVE_FOLDER, MASTER_EXTENSION, cachedLayouts);
            // ### Pages
            var cachedPages = CollectCachedPages();
            SaveCachedEntities(Project.RevisionNo, PAGES_FOLDER, PAGES_ARCHIVE_FOLDER, PAGE_EXTENSION, cachedPages);
            // ### Parts
            var cachedParts = CollectCachedParts();
            SaveCachedEntities(Project.RevisionNo, PARTS_FOLDER, PARTS_ARCHIVE_FOLDER, PART_EXTENSION, cachedParts);

        }




        private void SaveCachedEntities(long revNo, string folder, string archiveFolder, string entityExtension, IList<string> cachedEntities)
        {
            if (!cachedEntities.Any())
                return;
            // Ensure archive folder is there
            string pageArchiveFolder = Path.Combine(ProjectFolderName, archiveFolder);
            Directory.CreateDirectory(pageArchiveFolder);

            string pageFolder = Path.Combine(ProjectFolderName, folder);

            // Save each existing page under the current revison number
            // Then replace it with the cached page
            foreach (var cachedPage in cachedEntities)
            {
                var fileNameOnly = Path.GetFileNameWithoutExtension(cachedPage) ?? "unknown-entity";
                //Debug.WriteLine($"NEW PAGE: {fileNameOnly}");

                var archiveFileName = Path.Combine(pageArchiveFolder, fileNameOnly + "." + revNo);

                var newPageFile = Path.ChangeExtension(cachedPage, NEW_EXTENSION);
                newPageFile = Path.Combine(pageFolder, newPageFile);

                var cachedPageName = Path.Combine(pageFolder, cachedPage + entityExtension);

                if (File.Exists(newPageFile))
                {
                    if (File.Exists(cachedPageName))
                        File.Move(cachedPageName, archiveFileName);
                    File.Move(newPageFile, cachedPageName);
                }
            }
        }





        public FlowElementBase GetPart(string partName)
        {
            var fileName = Path.Combine(ProjectFolderName, PARTS_FOLDER, partName + PART_EXTENSION);
            if (!File.Exists(fileName))
                return null;
            string s = File.ReadAllText(fileName);
            FlowElementBase part = DeserializeFromJSON<FlowElementBase>(s, partName + "|" + fileName);
            return part;
        }


        public byte[] LoadImage(string imageName)
        {
            string imagePath = Path.Combine(ProjectFolderName, IMAGES_FOLDER, imageName);
            if (!File.Exists(imagePath))
                return null;
            byte[] bytArray = File.ReadAllBytes(imagePath);
            return bytArray;
        }




        public ImportImageResult CanImportImage(string imageFileName)
        {
            string imageName = Path.GetFileName(imageFileName);
            Debug.Assert(imageName != null, "imageName == null");
            string imagePath = Path.Combine(ProjectFolderName, IMAGES_FOLDER, imageName);
            if (File.Exists(imagePath))
                return ImportImageResult.DuplicateName;
            return ImportImageResult.OK;
        }



        public ImportImageResult ImportImage(string imageFileName)
        {
            string imageName = Path.GetFileName(imageFileName);
            Debug.Assert(imageName != null, "imageName == null");
            string targetFileName = Path.Combine(ProjectFolderName, IMAGES_FOLDER, imageName);
            if (File.Exists(targetFileName))
                return ImportImageResult.DuplicateName;
            try
            {
                File.Copy(imageFileName, targetFileName);
            }
            catch
            {
                return ImportImageResult.CopyError;
            }
            return ImportImageResult.OK;
        }




        public void ImportImages(IEnumerable<string> fileNames)
        {
            foreach (var imageName in fileNames)
            {
                ImportImage(imageName);
            }
        }



        public void DeleteImages(IList fileNames)
        {
            foreach (var imageName in fileNames)
            {
                string fileName = (string)imageName;

                string targetFileName = Path.Combine(ProjectFolderName, IMAGES_FOLDER, fileName);
                if (!File.Exists(targetFileName))
                    continue;
                File.Delete(targetFileName);
            }
        }



        #endregion


        #region Internal Housekeeping


        public void LoadMasterPages(string folderName)
        {
            MasterPages.Clear();
            folderName = Path.Combine(folderName, LAYOUTS_FOLDER);
            if (!Directory.Exists(folderName))
                return;
            var masterFiles = Directory.EnumerateFiles(folderName, "*" + MASTER_EXTENSION);
            if (!masterFiles.Any())
                return;
            foreach (var masterFile in masterFiles)
            {
                string nameOnly = Path.GetFileNameWithoutExtension(masterFile);
                MasterPages.Add(nameOnly, masterFile);
            }
        }


        #endregion


        #region Retrieve Data Sets


        public IList<string> GetAllPages(string projectFolder)
        {
            var pages = GetAllFiles(projectFolder, PAGES_FOLDER, PAGE_EXTENSION);
            HashSet<string> names = new HashSet<string>(pages);
            // Merge all new uncommitted pages
            var newPages = GetAllFiles(projectFolder, PAGES_FOLDER, NEW_EXTENSION);
            foreach (var pageName in newPages)
            {
                if (!names.Contains(pageName))
                    names.Add(pageName);
            }

            return names.ToList();
        }



        public IList<string> GetAllLayouts(string projectFolder)
        {
            var layouts = GetAllFiles(projectFolder, LAYOUTS_FOLDER, MASTER_EXTENSION);

            HashSet<string> names = new HashSet<string>(layouts);
            // Merge all new uncommitted pages
            var newPages = GetAllFiles(projectFolder, LAYOUTS_FOLDER, NEW_EXTENSION);
            foreach (var pageName in newPages)
            {
                if (!names.Contains(pageName))
                    names.Add(pageName);
            }
            return names.ToList();
        }



        public IList<string> GetAllParts(string projectFolder)
        {
            var parts = GetAllFiles(projectFolder, PARTS_FOLDER, PART_EXTENSION);

            HashSet<string> names = new HashSet<string>(parts);
            // Merge all new uncommitted pages
            var newPages = GetAllFiles(projectFolder, PARTS_FOLDER, NEW_EXTENSION);
            foreach (var pageName in newPages)
            {
                if (!names.Contains(pageName))
                    names.Add(pageName);
            }
            return names.ToList();
        }



        public IList<string> GetAllImages(string projectFolder)
        {
            return GetAllFiles(projectFolder, IMAGES_FOLDER);
        }


        public IList<string> CollectCachedPages()
        {
            return GetAllFiles(ProjectFolderName, PAGES_FOLDER, NEW_EXTENSION);
        }


        public IList<string> CollectCachedLayouts()
        {
            return GetAllFiles(ProjectFolderName, LAYOUTS_FOLDER, NEW_EXTENSION);
        }


        public IList<string> CollectCachedParts()
        {
            return GetAllFiles(ProjectFolderName, PARTS_FOLDER, NEW_EXTENSION);
        }


        public IList<string> GetAllFiles(string folderName, string subfolder, string extension = "")
        {
            var files = new List<string>();
            if (string.IsNullOrWhiteSpace(folderName))
                return files;
            folderName = Path.Combine(folderName, subfolder);
            if (!Directory.Exists(folderName))
                return files;
            var layoutFiles = Directory.EnumerateFiles(folderName, "*" + extension);
            var folderFiles = layoutFiles.ToList();
            files.AddRange(string.IsNullOrWhiteSpace(extension)
                ? folderFiles.Select(Path.GetFileName)
                : folderFiles.Select(Path.GetFileNameWithoutExtension));
            return files;
        }





        #endregion

    }
}