using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace WireFlow
{
    /// <summary>
    /// Keeps a WireFlow project in one SQLite database file (.wf): the project's settings, its pages, layouts and parts with
    /// their unsaved edits, their earlier revisions, and its images. It behaves as FlatFolderRepository does: Save* methods
    /// keep a document's unsaved edits as its draft, and SaveProject archives the saved versions and promotes the drafts.
    /// Documents are the same JSON as the folder's files. With ConvertFolderProjects, opening a folder project (.project)
    /// converts it into a .wf file of the same name beside it, once, and opens that.
    /// </summary>
    public class SqliteRepository : IFlowRepository
    {
        #region Constants

        public const string FILE_EXTENSION = ".wf";

        // PRAGMA application_id: "WFLW", marks the file as WireFlow's
        public const int APPLICATION_ID = 0x57464C57;

        // PRAGMA user_version: the file format; a higher number is a format of a later WireFlow
        public const int FORMAT_VERSION = 1;

        public const string PROJECT_KIND = "project";
        public const string PAGE_KIND = "page";
        public const string LAYOUT_KIND = "layout";
        public const string PART_KIND = "part";

        // Format 1. Names compare as file names did, ignoring case.
        // Documents: Saved is the JSON of the saved version (NULL until the first SaveProject), Draft the JSON of the unsaved
        // edits (NULL if none). Archive: the version that the save of project revision RevisionNo replaced.
        private const string SCHEMA = @"
CREATE TABLE Project (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),
    Json TEXT NOT NULL
);
CREATE TABLE Documents (
    Kind TEXT NOT NULL CHECK (Kind IN ('page', 'layout', 'part')),
    Name TEXT NOT NULL COLLATE NOCASE,
    Saved TEXT,
    Draft TEXT,
    PRIMARY KEY (Kind, Name)
);
CREATE TABLE Archive (
    Kind TEXT NOT NULL CHECK (Kind IN ('project', 'page', 'layout', 'part')),
    Name TEXT NOT NULL COLLATE NOCASE,
    RevisionNo INTEGER NOT NULL,
    Json TEXT NOT NULL,
    PRIMARY KEY (Kind, Name, RevisionNo)
);
CREATE TABLE Images (
    Name TEXT NOT NULL PRIMARY KEY COLLATE NOCASE,
    Data BLOB NOT NULL
);";

        #endregion


        #region Properties

        public string DocumentFileFilters => ConvertFolderProjects
            ? "WireFlow Project|*.wf|WireFlow Folder Project (converted to .wf when opened)|*.project|All files|*"
            : "WireFlow Project|*.wf|All files|*";

        public string DefaultFileExt { get; set; } = "wf";
        public string DefaultIndexNode { get; set; } = "index";
        public string DefaultBackground { get; set; } = FlowProject.DEFAULT_BACKGROUND;

        // Whether OpenProject converts a folder project (.project) into a .wf file; the Designer does, the Browser does not
        public bool ConvertFolderProjects { get; set; }

        public string ProjectFileName { get; private set; }

        private FlowProject Project { get; set; }


        public IList<string> Pages => GetNames(PAGE_KIND);
        public IList<string> Layouts => GetNames(LAYOUT_KIND);
        public IList<string> Parts => GetNames(PART_KIND);
        public IList<string> Images => ReadStrings("SELECT Name FROM Images ORDER BY Name");

        #endregion


        #region Events

        public Action<string> ProjectFileLoaded { get; set; }

        #endregion


        #region IFlowRepository

        public FlowProject NewProject(string docName)
        {
            if (string.IsNullOrWhiteSpace(docName))
                return null;
            docName = ResolveFileName(docName);
            // A new project is always a .wf file
            if (IsFolderProject(docName))
                docName = Path.ChangeExtension(docName, FILE_EXTENSION);
            else if (!string.Equals(Path.GetExtension(docName), FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                docName += FILE_EXTENSION;

            FlowProject project = new FlowProject()
            {
                Home = DefaultIndexNode,
                Background = DefaultBackground
            };
            CreateDatabase(docName, FlatFolderRepository.SerializeToJSON(project), null);
            ProjectFileName = docName;
            Project = project;

            // Default page
            CreatePage(DefaultIndexNode, null);
            return project;
        }



        public FlowProject OpenProject(string projectFileName)
        {
            if (string.IsNullOrWhiteSpace(projectFileName))
                return null;
            projectFileName = ResolveFileName(projectFileName);
            if (IsFolderProject(projectFileName))
            {
                if (!ConvertFolderProjects)
                    return null;
                string wfFileName = Path.ChangeExtension(projectFileName, FILE_EXTENSION);
                // A .wf file beside it was converted from it before, and is opened rather than replaced
                if (!File.Exists(wfFileName))
                    ConvertFolderProject(projectFileName, wfFileName);
                projectFileName = wfFileName;
            }
            if (!File.Exists(projectFileName))
                return null;

            FlowProject project;
            try
            {
                using (var connection = Connect(projectFileName, SqliteOpenMode.ReadWrite))
                {
                    if (!IsWireFlowFile(connection))
                    {
                        Debug.WriteLine($"{projectFileName} is not a WireFlow file of format {FORMAT_VERSION} or earlier");
                        return null;
                    }
                    string json = ExecuteScalar(connection, null, "SELECT Json FROM Project WHERE Id = 1") as string;
                    project = json == null ? null : FlatFolderRepository.DeserializeFromJSON<FlowProject>(json, projectFileName);
                }
            }
            catch (SqliteException ex)
            {
                Debug.WriteLine($"EXCEPTION in OpenProject({projectFileName}) {ex.GetType()}\n{ex.Message}");
                return null;
            }
            if (project == null)
                return null;

            ProjectFileName = projectFileName;
            Project = project;
            ProjectFileLoaded?.Invoke(ProjectFileName);
            return project;
        }



        public FlowPage GetMasterPage(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                return null;
            // The saved version, as the folder's .master file
            var master = ReadDocument<FlowPage>(LAYOUT_KIND, nodeName, "Saved");
            if (master != null)
                master.Name = nodeName;
            return master;
        }



        public FlowPage GetPage(string nodeName)
        {
            // The unsaved edits if any, as the folder's .new file, otherwise the saved version
            var page = ReadDocument<FlowPage>(PAGE_KIND, nodeName, "COALESCE(Draft, Saved)");
            if (page != null)
                page.Name = nodeName;
            return page;
        }



        public FlowElementBase GetPart(string partName)
        {
            // The saved version, as the folder's .part file
            return ReadDocument<FlowElementBase>(PART_KIND, partName, "Saved");
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


        public void DeletePage(string pageName) => DeleteDocument(PAGE_KIND, pageName);


        public void SavePage(FlowPage page)
        {
            Debug.Assert(page != null);
            SaveDraft(PAGE_KIND, page.Name, page);
        }


        public void RenamePage(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                return;
            if (string.IsNullOrWhiteSpace(newName))
                return;
            // Its archived versions keep the old name, as the folder's archive files do
            Execute("UPDATE Documents SET Name = $newName WHERE Kind = $kind AND Name = $oldName",
                ("$kind", PAGE_KIND), ("$oldName", oldName), ("$newName", newName));
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


        public void DeleteLayout(string pageName) => DeleteDocument(LAYOUT_KIND, pageName);


        public void SaveLayout(FlowPage page)
        {
            Debug.Assert(page != null);
            SaveDraft(LAYOUT_KIND, page.Name, page);
        }


        public void RenameLayout(string oldName, string newName)
        {
            // As in FlatFolderRepository: pages refer to their layout by name
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


        public void DeletePart(string partName) => DeleteDocument(PART_KIND, partName);


        public void SavePart(FlowPart part)
        {
            Debug.Assert(part != null);
            SaveDraft(PART_KIND, part.Name, part);
        }


        public void RenamePart(string oldName, string newName)
        {
            // As in FlatFolderRepository: elements refer to their part by name
            throw new NotImplementedException();
        }



        public void SaveProject()
        {
            if (Project == null || ProjectFileName == null)
                return;
            Project.RevisionNo++;
            try
            {
                string projectName = Path.GetFileNameWithoutExtension(ProjectFileName);
                using (var connection = Connect(ProjectFileName, SqliteOpenMode.ReadWrite))
                using (var transaction = connection.BeginTransaction())
                {
                    // Archive the project and each saved document that has unsaved edits under the new revision, then
                    // promote the edits
                    ExecuteNonQuery(connection, transaction, @"
INSERT OR REPLACE INTO Archive (Kind, Name, RevisionNo, Json)
    SELECT $projectKind, $projectName, $revisionNo, Json FROM Project WHERE Id = 1;
UPDATE Project SET Json = $json WHERE Id = 1;
INSERT OR REPLACE INTO Archive (Kind, Name, RevisionNo, Json)
    SELECT Kind, Name, $revisionNo, Saved FROM Documents WHERE Draft IS NOT NULL AND Saved IS NOT NULL;
UPDATE Documents SET Saved = Draft, Draft = NULL WHERE Draft IS NOT NULL;",
                        ("$projectKind", PROJECT_KIND), ("$projectName", projectName), ("$revisionNo", Project.RevisionNo),
                        ("$json", FlatFolderRepository.SerializeToJSON(Project)));
                    transaction.Commit();
                }
            }
            catch
            {
                Project.RevisionNo--;
                throw;
            }
        }



        public byte[] LoadImage(string imageName)
        {
            return ExecuteScalar("SELECT Data FROM Images WHERE Name = $name", ("$name", imageName)) as byte[];
        }


        public ImportImageResult CanImportImage(string imageFileName)
        {
            string imageName = Path.GetFileName(imageFileName);
            Debug.Assert(imageName != null, "imageName == null");
            return HasImage(imageName) ? ImportImageResult.DuplicateName : ImportImageResult.OK;
        }


        public ImportImageResult ImportImage(string imageFileName)
        {
            string imageName = Path.GetFileName(imageFileName);
            Debug.Assert(imageName != null, "imageName == null");
            if (HasImage(imageName))
                return ImportImageResult.DuplicateName;
            try
            {
                byte[] data = File.ReadAllBytes(imageFileName);
                Execute("INSERT INTO Images (Name, Data) VALUES ($name, $data)", ("$name", imageName), ("$data", data));
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
                Execute("DELETE FROM Images WHERE Name = $name", ("$name", (string)imageName));
            }
        }

        #endregion


        #region Folder Projects

        public static bool IsFolderProject(string fileName) =>
            string.Equals(Path.GetExtension(fileName), FlatFolderRepository.PROJECT_EXTENSION, StringComparison.OrdinalIgnoreCase);


        /// <summary>
        /// Writes the folder project of a .project file into a new .wf file: its settings, pages, layouts and parts with their
        /// unsaved edits (.new), their archived revisions, and its images. Documents keep their JSON as it is in the files.
        /// The folder is left as it is.
        /// </summary>
        public static void ConvertFolderProject(string projectFileName, string wfFileName)
        {
            string projectJson = File.ReadAllText(projectFileName);
            if (FlatFolderRepository.DeserializeFromJSON<FlowProject>(projectJson, projectFileName) == null)
                throw new InvalidDataException($"{projectFileName} is not a WireFlow project.");
            string folder = Path.GetDirectoryName(projectFileName);

            CreateDatabase(wfFileName, projectJson, (connection, transaction) =>
            {
                ConvertDocuments(connection, transaction, PAGE_KIND, Path.Combine(folder, FlatFolderRepository.PAGES_FOLDER),
                    FlatFolderRepository.PAGE_EXTENSION);
                ConvertDocuments(connection, transaction, LAYOUT_KIND, Path.Combine(folder, FlatFolderRepository.LAYOUTS_FOLDER),
                    FlatFolderRepository.MASTER_EXTENSION);
                ConvertDocuments(connection, transaction, PART_KIND, Path.Combine(folder, FlatFolderRepository.PARTS_FOLDER),
                    FlatFolderRepository.PART_EXTENSION);

                ConvertArchive(connection, transaction, PROJECT_KIND, Path.Combine(folder, FlatFolderRepository.PROJECT_ARCHIVE_FOLDER));
                ConvertArchive(connection, transaction, PAGE_KIND, Path.Combine(folder, FlatFolderRepository.PAGES_ARCHIVE_FOLDER));
                ConvertArchive(connection, transaction, LAYOUT_KIND, Path.Combine(folder, FlatFolderRepository.LAYOUTS_ARCHIVE_FOLDER));
                ConvertArchive(connection, transaction, PART_KIND, Path.Combine(folder, FlatFolderRepository.PARTS_ARCHIVE_FOLDER));

                foreach (var imageFile in EnumerateFiles(Path.Combine(folder, FlatFolderRepository.IMAGES_FOLDER)))
                {
                    ExecuteNonQuery(connection, transaction, "INSERT INTO Images (Name, Data) VALUES ($name, $data)",
                        ("$name", Path.GetFileName(imageFile)), ("$data", File.ReadAllBytes(imageFile)));
                }
            });
        }


        private static void ConvertDocuments(SqliteConnection connection, SqliteTransaction transaction, string kind, string folder,
            string extension)
        {
            foreach (var file in EnumerateFiles(folder))
            {
                string fileExtension = Path.GetExtension(file);
                string column;
                if (string.Equals(fileExtension, extension, StringComparison.OrdinalIgnoreCase))
                    column = "Saved";
                else if (string.Equals(fileExtension, FlatFolderRepository.NEW_EXTENSION, StringComparison.OrdinalIgnoreCase))
                    column = "Draft";
                else
                    continue;
                ExecuteNonQuery(connection, transaction,
                    $"INSERT INTO Documents (Kind, Name, {column}) VALUES ($kind, $name, $json) " +
                    $"ON CONFLICT (Kind, Name) DO UPDATE SET {column} = excluded.{column}",
                    ("$kind", kind), ("$name", Path.GetFileNameWithoutExtension(file)), ("$json", File.ReadAllText(file)));
            }
        }


        // The archive folders hold <name>.<revision number> files
        private static void ConvertArchive(SqliteConnection connection, SqliteTransaction transaction, string kind, string folder)
        {
            foreach (var file in EnumerateFiles(folder))
            {
                string fileName = Path.GetFileName(file);
                int dot = fileName.LastIndexOf('.');
                if (dot <= 0 || !long.TryParse(fileName.Substring(dot + 1), out long revisionNo))
                    continue;
                ExecuteNonQuery(connection, transaction,
                    "INSERT OR REPLACE INTO Archive (Kind, Name, RevisionNo, Json) VALUES ($kind, $name, $revisionNo, $json)",
                    ("$kind", kind), ("$name", fileName.Substring(0, dot)), ("$revisionNo", revisionNo),
                    ("$json", File.ReadAllText(file)));
            }
        }


        private static IEnumerable<string> EnumerateFiles(string folder) =>
            Directory.Exists(folder) ? Directory.EnumerateFiles(folder) : Enumerable.Empty<string>();

        #endregion


        #region Database

        // Pooling is off so that a .wf file is closed as soon as an operation is done: a pooled connection keeps it open,
        // and the file could not be replaced, moved or deleted while the program runs
        private static SqliteConnection Connect(string fileName, SqliteOpenMode mode)
        {
            var builder = new SqliteConnectionStringBuilder()
            {
                DataSource = fileName,
                Mode = mode,
                Pooling = false
            };
            var connection = new SqliteConnection(builder.ToString());
            connection.Open();
            return connection;
        }


        // Builds the file under a temporary name and moves it into place, so that a failure leaves no partial .wf file
        private static void CreateDatabase(string fileName, string projectJson, Action<SqliteConnection, SqliteTransaction> fill)
        {
            string tempFileName = fileName + ".tmp";
            File.Delete(tempFileName);
            try
            {
                using (var connection = Connect(tempFileName, SqliteOpenMode.ReadWriteCreate))
                using (var transaction = connection.BeginTransaction())
                {
                    ExecuteNonQuery(connection, transaction,
                        SCHEMA + $"\nPRAGMA application_id = {APPLICATION_ID};\nPRAGMA user_version = {FORMAT_VERSION};");
                    ExecuteNonQuery(connection, transaction, "INSERT INTO Project (Id, Json) VALUES (1, $json)", ("$json", projectJson));
                    fill?.Invoke(connection, transaction);
                    transaction.Commit();
                }
                File.Move(tempFileName, fileName, true);
            }
            catch
            {
                try
                {
                    File.Delete(tempFileName);
                }
                catch (IOException)
                {
                }
                throw;
            }
        }


        private static bool IsWireFlowFile(SqliteConnection connection)
        {
            long applicationId = (long)ExecuteScalar(connection, null, "PRAGMA application_id");
            long formatVersion = (long)ExecuteScalar(connection, null, "PRAGMA user_version");
            return applicationId == APPLICATION_ID && formatVersion >= 1 && formatVersion <= FORMAT_VERSION;
        }


        private T ReadDocument<T>(string kind, string name, string column)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default(T);
            string json = ExecuteScalar($"SELECT {column} FROM Documents WHERE Kind = $kind AND Name = $name",
                ("$kind", kind), ("$name", name)) as string;
            return json == null ? default(T) : FlatFolderRepository.DeserializeFromJSON<T>(json, kind + "|" + name);
        }


        private void SaveDraft(string kind, string name, object document)
        {
            string json = FlatFolderRepository.SerializeToJSON(document);
            // A document that cannot be serialized keeps its previous draft
            if (json == null)
                return;
            Execute("INSERT INTO Documents (Kind, Name, Draft) VALUES ($kind, $name, $json) " +
                    "ON CONFLICT (Kind, Name) DO UPDATE SET Draft = excluded.Draft",
                ("$kind", kind), ("$name", name), ("$json", json));
        }


        // Its archived versions stay, as the folder's archive files do
        private void DeleteDocument(string kind, string name)
        {
            Execute("DELETE FROM Documents WHERE Kind = $kind AND Name = $name", ("$kind", kind), ("$name", name));
        }


        private bool HasImage(string imageName) =>
            ExecuteScalar("SELECT 1 FROM Images WHERE Name = $name", ("$name", imageName)) != null;


        private IList<string> GetNames(string kind) =>
            ReadStrings("SELECT Name FROM Documents WHERE Kind = $kind ORDER BY Name", ("$kind", kind));


        private IList<string> ReadStrings(string sql, params (string Name, object Value)[] parameters)
        {
            var result = new List<string>();
            if (ProjectFileName == null)
                return result;
            using (var connection = Connect(ProjectFileName, SqliteOpenMode.ReadWrite))
            using (var command = CreateCommand(connection, null, sql, parameters))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    result.Add(reader.GetString(0));
            }
            return result;
        }


        private object ExecuteScalar(string sql, params (string Name, object Value)[] parameters)
        {
            if (ProjectFileName == null)
                return null;
            using (var connection = Connect(ProjectFileName, SqliteOpenMode.ReadWrite))
                return ExecuteScalar(connection, null, sql, parameters);
        }


        private void Execute(string sql, params (string Name, object Value)[] parameters)
        {
            if (ProjectFileName == null)
                return;
            using (var connection = Connect(ProjectFileName, SqliteOpenMode.ReadWrite))
                ExecuteNonQuery(connection, null, sql, parameters);
        }


        // NULL comes back as null rather than DBNull
        private static object ExecuteScalar(SqliteConnection connection, SqliteTransaction transaction, string sql,
            params (string Name, object Value)[] parameters)
        {
            using (var command = CreateCommand(connection, transaction, sql, parameters))
            {
                object result = command.ExecuteScalar();
                return result is DBNull ? null : result;
            }
        }


        private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string sql,
            params (string Name, object Value)[] parameters)
        {
            using (var command = CreateCommand(connection, transaction, sql, parameters))
                command.ExecuteNonQuery();
        }


        private static SqliteCommand CreateCommand(SqliteConnection connection, SqliteTransaction transaction, string sql,
            (string Name, object Value)[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            foreach (var parameter in parameters)
                command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
            return command;
        }


        // A bare file name is in the current project's folder, as in FlatFolderRepository
        private string ResolveFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(fileName)) && ProjectFileName != null)
                fileName = Path.Combine(Path.GetDirectoryName(ProjectFileName), fileName);
            return Path.GetFullPath(fileName);
        }

        #endregion
    }
}
