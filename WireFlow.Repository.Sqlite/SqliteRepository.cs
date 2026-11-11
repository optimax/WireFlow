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

        // PRAGMA user_version: the file format. 2 is the JSON of FlowJson, without .NET type names; 3 adds each document's
        // Key, which the project's page order names. A format 2 file is upgraded to 3 as it is opened (UpgradeToFormat3).
        // Format 1 files, with $type tags, were upgraded once by Documents\upgrade-wf-to-format-2.cs.
        public const int FORMAT_VERSION = 3;
        private const int FORMAT_2 = 2;

        public const string PROJECT_KIND = "project";
        public const string PAGE_KIND = "page";
        public const string LAYOUT_KIND = "layout";
        public const string PART_KIND = "part";

        // Format 3. Key is a document's own, a GUID of 32 hex digits the program gives it, which never changes and is never
        // shown; Name is what the user calls it, and may change. Archive keeps the key and the name of each version.
        // Names compare as file names did, ignoring case.
        // Documents: Saved is the JSON of the saved version (NULL until the first SaveProject), Draft the JSON of the unsaved
        // edits (NULL if none). Archive: the version that the save of project revision RevisionNo replaced.
        private const string SCHEMA = @"
CREATE TABLE Project (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),
    Json TEXT NOT NULL
);
CREATE TABLE Documents (
    Key TEXT NOT NULL PRIMARY KEY,
    Kind TEXT NOT NULL CHECK (Kind IN ('page', 'layout', 'part')),
    Name TEXT NOT NULL COLLATE NOCASE,
    Saved TEXT,
    Draft TEXT,
    UNIQUE (Kind, Name)
);
CREATE TABLE Archive (
    Kind TEXT NOT NULL CHECK (Kind IN ('project', 'page', 'layout', 'part')),
    Name TEXT NOT NULL COLLATE NOCASE,
    RevisionNo INTEGER NOT NULL,
    Json TEXT NOT NULL,
    Key TEXT,
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


        // The pages' keys, in the project's order (FlowProject.OrderPages), the first the entry point
        public IList<string> Pages => Project == null ? GetKeys(PAGE_KIND) : Project.OrderPages(GetKeys(PAGE_KIND));
        // The layouts' keys, by name
        public IList<string> Layouts => GetKeys(LAYOUT_KIND);
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
                    int format = FormatOf(connection);
                    if (format == FORMAT_2)
                        UpgradeToFormat3(connection);
                    else if (format != FORMAT_VERSION)
                    {
                        Debug.WriteLine($"{projectFileName} is not a WireFlow file of format {FORMAT_VERSION}");
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



        public FlowPage GetMasterPage(string layoutKey)
        {
            // As a page: the unsaved edits if any, otherwise the saved version, so that a layout's edits show on the pages set on it
            return ReadKeyed(LAYOUT_KIND, layoutKey, "COALESCE(Draft, Saved)");
        }


        public string LayoutName(string layoutKey) => NameOf(LAYOUT_KIND, layoutKey);


        public string LayoutKey(string layoutName) => KeyOf(LAYOUT_KIND, layoutName);



        public FlowPage GetPage(string pageKey)
        {
            // The unsaved edits if any, as the folder's .new file, otherwise the saved version
            return ReadKeyed(PAGE_KIND, pageKey, "COALESCE(Draft, Saved)");
        }


        public string PageName(string pageKey) => NameOf(PAGE_KIND, pageKey);


        public string PageKey(string pageName) => KeyOf(PAGE_KIND, pageName);


        // A page or layout by its key, with its name and key
        private FlowPage ReadKeyed(string kind, string key, string column)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var row = ReadRow($"SELECT Name, {column} FROM Documents WHERE Kind = $kind AND Key = $key", ("$kind", kind), ("$key", key));
            var page = row.Json == null ? null : FlatFolderRepository.DeserializeFromJSON<FlowPage>(row.Json, kind + "|" + row.Name);
            if (page != null)
            {
                page.Name = row.Name;
                page.Key = key;
            }
            return page;
        }


        private string NameOf(string kind, string key) => string.IsNullOrWhiteSpace(key) ? null :
            ExecuteScalar("SELECT Name FROM Documents WHERE Kind = $kind AND Key = $key", ("$kind", kind), ("$key", key)) as string;


        private string KeyOf(string kind, string name) => string.IsNullOrWhiteSpace(name) ? null :
            ExecuteScalar("SELECT Key FROM Documents WHERE Kind = $kind AND Name = $name", ("$kind", kind), ("$name", name)) as string;


        // A page's or layout's draft, under its key, with its name; one without a key (read elsewhere) is the one of its name, or a new one
        private void SaveKeyed(string kind, FlowPage page)
        {
            Debug.Assert(page != null);
            if (string.IsNullOrWhiteSpace(page.Key))
                page.Key = KeyOf(kind, page.Name) ?? NewKey();
            string json = FlatFolderRepository.SerializeToJSON(page);
            // A document that cannot be serialized keeps its previous draft
            if (json == null)
                return;
            Execute("INSERT INTO Documents (Key, Kind, Name, Draft) VALUES ($key, $kind, $name, $json) " +
                    "ON CONFLICT (Key) DO UPDATE SET Name = excluded.Name, Draft = excluded.Draft",
                ("$key", page.Key), ("$kind", kind), ("$name", page.Name), ("$json", json));
        }


        // Its key stays, and with it its place, the tabs showing it and what refers to it by key. Its archived versions keep their names.
        private void RenameKeyed(string kind, string key, string newName)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(newName))
                return;
            Execute("UPDATE Documents SET Name = $newName WHERE Kind = $kind AND Key = $key", ("$kind", kind), ("$key", key), ("$newName", newName));
        }



        public FlowElementBase GetPart(string partName)
        {
            // The saved version, as the folder's .part file; a part is a FlowPart, whose root names no class of its own
            return ReadDocument<FlowPart>(PART_KIND, partName, "Saved");
        }



        public string CreatePage(string pageName, string layoutName)
        {
            FlowPage page = new FlowPage()
            {
                Key = NewKey(),
                Name = pageName,
                Master = layoutName,
            };
            FlowLayer userLayer = new FlowLayer()
            {
                Name = "Layer1",
            };
            page.Layers.Add(userLayer);
            // A new page comes last
            var order = Pages.ToList();
            SavePage(page);
            order.Add(page.Key);
            SetPageOrder(order);
            return page.Key;
        }


        public void DeletePage(string pageKey)
        {
            Execute("DELETE FROM Documents WHERE Kind = $kind AND Key = $key", ("$kind", PAGE_KIND), ("$key", pageKey));
            SetPageOrder(Pages);
        }


        public void SavePage(FlowPage page) => SaveKeyed(PAGE_KIND, page);


        public void RenamePage(string pageKey, string newName) => RenameKeyed(PAGE_KIND, pageKey, newName);


        // Written at once, as creating and deleting a page are, not kept for SaveProject
        public void SetPageOrder(IList<string> pageKeys)
        {
            if (Project == null || ProjectFileName == null)
                return;
            Project.PageOrder = pageKeys.ToList();
            if (pageKeys.Count > 0)
                Project.Home = pageKeys[0];
            Execute("UPDATE Project SET Json = $json WHERE Id = 1", ("$json", FlatFolderRepository.SerializeToJSON(Project)));
        }



        public string CreateLayout(string layoutName, string masterName)
        {
            FlowPage page = new FlowPage()
            {
                Key = NewKey(),
                Name = layoutName,
                Master = masterName,
            };
            FlowLayer userLayer = new FlowLayer()
            {
                Name = "Master1",
            };
            page.Layers.Add(userLayer);
            SaveLayout(page);
            return page.Key;
        }


        public void DeleteLayout(string layoutKey) =>
            Execute("DELETE FROM Documents WHERE Kind = $kind AND Key = $key", ("$kind", LAYOUT_KIND), ("$key", layoutKey));


        public void SaveLayout(FlowPage page) => SaveKeyed(LAYOUT_KIND, page);


        // What refers to the layout by its name (a page's Master) is the caller's to change (WireFlowEngine.RetargetMasters)
        public void RenameLayout(string layoutKey, string newName) => RenameKeyed(LAYOUT_KIND, layoutKey, newName);



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
INSERT OR REPLACE INTO Archive (Kind, Name, RevisionNo, Json, Key)
    SELECT Kind, Name, $revisionNo, Saved, Key FROM Documents WHERE Draft IS NOT NULL AND Saved IS NOT NULL;
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
        /// unsaved edits (.new), their archived revisions, and its images. Each document is written in the current JSON
        /// (see CurrentJson). The folder is left as it is.
        /// </summary>
        public static void ConvertFolderProject(string projectFileName, string wfFileName)
        {
            var project = FlatFolderRepository.DeserializeFromJSON<FlowProject>(File.ReadAllText(projectFileName), projectFileName);
            if (project == null)
                throw new InvalidDataException($"{projectFileName} is not a WireFlow project.");
            string folder = Path.GetDirectoryName(projectFileName);

            CreateDatabase(wfFileName, FlatFolderRepository.SerializeToJSON(project), (connection, transaction) =>
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
                KeyArchive(connection, transaction);
                KeyPageOrder(connection, transaction);

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
                    $"INSERT INTO Documents (Key, Kind, Name, {column}) VALUES ($key, $kind, $name, $json) " +
                    $"ON CONFLICT (Kind, Name) DO UPDATE SET {column} = excluded.{column}",
                    ("$key", NewKey()), ("$kind", kind), ("$name", Path.GetFileNameWithoutExtension(file)), ("$json", CurrentJson(kind, file)));
            }
        }


        /// <summary>A document's key: a GUID of 32 hex digits, without hyphens or braces.</summary>
        public static string NewKey() => Guid.NewGuid().ToString("N");


        // Format 2 to 3, in place, in one transaction: each document gets a key, its archived versions too, and the project's
        // page order and home page name pages by key
        private static void UpgradeToFormat3(SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                ExecuteNonQuery(connection, transaction, @"
CREATE TABLE Documents3 (
    Key TEXT NOT NULL PRIMARY KEY,
    Kind TEXT NOT NULL CHECK (Kind IN ('page', 'layout', 'part')),
    Name TEXT NOT NULL COLLATE NOCASE,
    Saved TEXT,
    Draft TEXT,
    UNIQUE (Kind, Name)
);
INSERT INTO Documents3 (Key, Kind, Name, Saved, Draft) SELECT lower(hex(randomblob(16))), Kind, Name, Saved, Draft FROM Documents;
DROP TABLE Documents;
ALTER TABLE Documents3 RENAME TO Documents;
ALTER TABLE Archive ADD COLUMN Key TEXT;");
                KeyArchive(connection, transaction);
                KeyPageOrder(connection, transaction);
                ExecuteNonQuery(connection, transaction, $"PRAGMA user_version = {FORMAT_VERSION};");
                transaction.Commit();
            }
        }


        // The archived versions of each document get its key, by kind and name
        private static void KeyArchive(SqliteConnection connection, SqliteTransaction transaction) =>
            ExecuteNonQuery(connection, transaction,
                "UPDATE Archive SET Key = (SELECT d.Key FROM Documents d WHERE d.Kind = Archive.Kind AND d.Name = Archive.Name) WHERE Key IS NULL");


        // The project's page order and home page, which named pages, name them by key; names of no page are dropped
        private static void KeyPageOrder(SqliteConnection connection, SqliteTransaction transaction)
        {
            string json = ExecuteScalar(connection, transaction, "SELECT Json FROM Project WHERE Id = 1") as string;
            var project = json == null ? null : FlatFolderRepository.DeserializeFromJSON<FlowProject>(json, "project");
            if (project == null)
                return;
            string KeyOf(string name) => string.IsNullOrWhiteSpace(name) ? null : ExecuteScalar(connection, transaction,
                "SELECT Key FROM Documents WHERE Kind = $kind AND Name = $name", ("$kind", PAGE_KIND), ("$name", name)) as string;
            if (project.PageOrder != null)
                project.PageOrder = project.PageOrder.Select(KeyOf).Where(k => k != null).Distinct().ToList();
            project.Home = KeyOf(project.Home) ?? project.PageOrder?.FirstOrDefault() ?? project.Home;
            ExecuteNonQuery(connection, transaction, "UPDATE Project SET Json = $json WHERE Id = 1",
                ("$json", FlatFolderRepository.SerializeToJSON(project)));
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
                    ("$json", CurrentJson(kind, file)));
            }
        }


        // A document of a folder project, read and written again in the current JSON. Elements there carry their Type as
        // well, which is all the current JSON needs. A file that cannot be read keeps its JSON, so that nothing is lost.
        private static string CurrentJson(string kind, string file)
        {
            string json = File.ReadAllText(file);
            object document;
            if (kind == PROJECT_KIND)
                document = FlatFolderRepository.DeserializeFromJSON<FlowProject>(json, file);
            else if (kind == PART_KIND)
                document = FlatFolderRepository.DeserializeFromJSON<FlowPart>(json, file);
            else
                document = FlatFolderRepository.DeserializeFromJSON<FlowPage>(json, file);
            return document == null ? json : FlatFolderRepository.SerializeToJSON(document) ?? json;
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


        // The format of a WireFlow file, or -1 for another file
        private static int FormatOf(SqliteConnection connection)
        {
            long applicationId = (long)ExecuteScalar(connection, null, "PRAGMA application_id");
            long formatVersion = (long)ExecuteScalar(connection, null, "PRAGMA user_version");
            return applicationId == APPLICATION_ID ? (int)formatVersion : -1;
        }


        // A row's name and JSON
        private (string Name, string Json) ReadRow(string sql, params (string Name, object Value)[] parameters)
        {
            if (ProjectFileName == null)
                return (null, null);
            using (var connection = Connect(ProjectFileName, SqliteOpenMode.ReadWrite))
            using (var command = CreateCommand(connection, null, sql, parameters))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                    return (null, null);
                return (reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1));
            }
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
            Execute("INSERT INTO Documents (Key, Kind, Name, Draft) VALUES ($key, $kind, $name, $json) " +
                    "ON CONFLICT (Kind, Name) DO UPDATE SET Draft = excluded.Draft",
                ("$key", NewKey()), ("$kind", kind), ("$name", name), ("$json", json));
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


        // By name, as the order of the pages ends with the pages it does not name
        private IList<string> GetKeys(string kind) =>
            ReadStrings("SELECT Key FROM Documents WHERE Kind = $kind ORDER BY Name", ("$kind", kind));


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
