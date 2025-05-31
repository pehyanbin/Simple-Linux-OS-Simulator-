using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using testonly.NewFolder;

namespace FileStorageSystem
{
    public class FileManager
    {
        private Folder _rootFolder;
        private Folder _currentFolder;
        private HistoryLogger _historyLogger;
        private static readonly string StorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_system.json");

        [JsonPropertyName("rootFolder")]
        public Folder RootFolder
        {
            get => _rootFolder;
            set => _rootFolder = value;
        }

        [JsonIgnore]
        public Folder CurrentFolder
        {
            get => _currentFolder;
            set => _currentFolder = value;
        }

        [JsonPropertyName("currentFolderPath")]
        public string CurrentFolderPath { get; set; }

        [JsonIgnore]
        public HistoryLogger Historylogger
        {
            get => _historyLogger;
            set => _historyLogger = value;
        }

        public FileManager()
        {
            RootFolder = new Folder("root", null);
            CurrentFolder = RootFolder;
            _historyLogger = new HistoryLogger();
            CurrentFolderPath = RootFolder.GetFullPath();
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "root"));
            Console.WriteLine("File System Initialized. Type 'help' for commands.");
        }

        [JsonConstructor]
        public FileManager(Folder rootFolder, string currentFolderPath)
        {
            RootFolder = rootFolder;
            _historyLogger = new HistoryLogger();
            CurrentFolderPath = currentFolderPath;
            RebuildParentReferences(RootFolder, null);
            CurrentFolder = string.IsNullOrEmpty(currentFolderPath) ? RootFolder : GetEntityFromPath(currentFolderPath) as Folder ?? RootFolder;
            EnsureDirectoriesExist(RootFolder);
            Console.WriteLine("File System Initialized. Type 'help' for commands.");
        }

        private void EnsureDirectoriesExist(Folder folder)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(folderPath);
            foreach (var entity in folder.Contents)
            {
                if (entity is Folder subFolder)
                {
                    EnsureDirectoriesExist(subFolder);
                }
                else if (entity is File file)
                {
                    file.UpdateFilePath();
                }
            }
        }

        public static FileManager Load()
        {
            try
            {
                if (System.IO.File.Exists(StorageFilePath))
                {
                    string json = System.IO.File.ReadAllText(StorageFilePath);
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new FileSystemEntityConverter() }
                    };
                    return JsonSerializer.Deserialize<FileManager>(json, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file system: {ex.Message}. Initializing new file system.");
            }
            return new FileManager();
        }

        public void Save()
        {
            try
            {
                CurrentFolderPath = CurrentFolder.GetFullPath();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new FileSystemEntityConverter() }
                };
                string json = JsonSerializer.Serialize(this, options);
                System.IO.File.WriteAllText(StorageFilePath, json);
                Console.WriteLine("File system metadata saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file system: {ex.Message}");
            }
        }

        private void RebuildParentReferences(Folder folder, Folder parent)
        {
            folder.ParentFolder = parent;
            foreach (var entity in folder.Contents)
            {
                entity.ParentFolder = folder;
                if (entity is Folder subFolder)
                {
                    RebuildParentReferences(subFolder, folder);
                }
                else if (entity is File file)
                {
                    file.UpdateFilePath();
                }
            }
        }

        public FileSystemEntity GetEntityFromPath(string path)
        {
            Folder startingFolder = CurrentFolder;
            if (path.StartsWith("/"))
            {
                startingFolder = RootFolder;
                path = path.Substring(1);
            }

            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            FileSystemEntity currentEntity = startingFolder;

            if (pathParts.Length == 0 && path == "/") return RootFolder;

            foreach (string part in pathParts)
            {
                if (currentEntity is Folder folder)
                {
                    if (part == ".") continue;
                    if (part == "..")
                    {
                        if (folder.ParentFolder != null)
                        {
                            currentEntity = folder.ParentFolder;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        currentEntity = folder.GetEntity(part);
                        if (currentEntity == null)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            return currentEntity;
        }

        public Folder GetParentFolderFromPath(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directoryPath))
            {
                return CurrentFolder;
            }
            return GetEntityFromPath(directoryPath) as Folder;
        }

        public string GetEntityNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        public void CreateFile(string path, string content = "")
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path);
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; }

                string fileName = GetEntityNameFromPath(path);
                if (string.IsNullOrEmpty(fileName)) { Console.WriteLine("Error: File name cannot be empty."); return; }

                if (parent.GetEntity(fileName) != null)
                {
                    Console.WriteLine($"Error: File '{fileName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                string physicalParentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, parent.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                Directory.CreateDirectory(physicalParentPath);

                File newFile = new File(fileName, parent, content);
                parent.AddEntity(newFile);
                Console.WriteLine($"File '{newFile.Name}' created in '{parent.GetFullPath()}'.");
                _historyLogger.LogAccess(newFile.GetFullPath(), DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void CreateFolder(string path)
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path);
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; }

                string folderName = GetEntityNameFromPath(path);
                if (string.IsNullOrEmpty(folderName)) { Console.WriteLine("Error: Folder name cannot be empty."); return; }

                if (parent.GetEntity(folderName) != null)
                {
                    Console.WriteLine($"Error: Folder '{folderName}' already exists in '{parent.GetFullPath()}'.");
                    return;
                }

                string physicalParentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, parent.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                string newFolderPath = Path.Combine(physicalParentPath, folderName);
                Directory.CreateDirectory(newFolderPath);

                Folder newFolder = new Folder(folderName, parent);
                parent.AddEntity(newFolder);
                Console.WriteLine($"Folder '{newFolder.Name}' created in '{parent.GetFullPath()}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void DeleteEntity(string path)
        {
            try
            {
                FileSystemEntity entityToDelete = GetEntityFromPath(path);
                if (entityToDelete == null) { Console.WriteLine("Error: File or folder not found."); return; }

                if (entityToDelete == RootFolder)
                {
                    Console.WriteLine("Error: Cannot delete the root folder.");
                    return;
                }

                if (entityToDelete.ParentFolder != null)
                {
                    if (entityToDelete is File file)
                    {
                        file.DeletePhysicalFile();
                    }
                    else if (entityToDelete is Folder folder)
                    {
                        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                        }
                    }
                    entityToDelete.ParentFolder.RemoveEntity(entityToDelete);
                    Console.WriteLine($"'{entityToDelete.Name}' deleted.");
                }
                else
                {
                    Console.WriteLine("Error: Cannot delete entity without a parent folder (should not happen for non-root).");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void RenameEntity(string path, string newName)
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path);
                if (entity == null) { Console.WriteLine("Error: File or folder not found."); return; }

                if (entity == RootFolder)
                {
                    Console.WriteLine("Error: Cannot rename the root folder.");
                    return;
                }

                string oldName = entity.Name;
                entity.Rename(newName);
                if (entity is File file)
                {
                    file.UpdateFilePath();
                }
                else if (entity is Folder folder)
                {
                    string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                    string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                    if (Directory.Exists(oldPath))
                    {
                        Directory.Move(oldPath, newPath);
                    }
                    UpdateFolderFilePaths(folder);
                }
                Console.WriteLine($"'{oldName}' renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void UpdateFolderFilePaths(Folder folder)
        {
            foreach (var entity in folder.Contents)
            {
                if (entity is File file)
                {
                    file.UpdateFilePath();
                }
                else if (entity is Folder subFolder)
                {
                    UpdateFolderFilePaths(subFolder);
                }
            }
        }

        public void MoveEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath);
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; }

                if (sourceEntity == RootFolder)
                {
                    Console.WriteLine("Error: Cannot move the root folder.");
                    return;
                }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder;
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; }

                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                string sourcePhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceEntity.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFolder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()), sourceEntity.Name);
                if (sourceEntity is File && System.IO.File.Exists(sourcePhysicalPath))
                {
                    System.IO.File.Move(sourcePhysicalPath, destPhysicalPath);
                }
                else if (sourceEntity is Folder && Directory.Exists(sourcePhysicalPath))
                {
                    Directory.Move(sourcePhysicalPath, destPhysicalPath);
                }

                sourceEntity.ParentFolder.RemoveEntity(sourceEntity);
                destinationFolder.AddEntity(sourceEntity);
                if (sourceEntity is File file)
                {
                    file.UpdateFilePath();
                }
                else if (sourceEntity is Folder folder)
                {
                    UpdateFolderFilePaths(folder);
                }
                Console.WriteLine($"Moved '{sourceEntity.Name}' from '{sourceEntity.ParentFolder.GetFullPath()}' to '{destinationFolder.GetFullPath()}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving entity: {ex.Message}");
            }
        }

        public void CopyEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath);
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder;
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; }

                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder.");
                    return;
                }

                string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFolder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()), sourceEntity.Name);
                if (sourceEntity is File sourceFile)
                {
                    File newFile = new File(sourceFile.Name, destinationFolder, sourceFile.GetContent());
                    destinationFolder.AddEntity(newFile);
                    Console.WriteLine($"Copied file '{sourceFile.Name}' to '{destinationFolder.GetFullPath()}'.");
                }
                else if (sourceEntity is Folder sourceFolder)
                {
                    Directory.CreateDirectory(destPhysicalPath);
                    Folder newFolder = new Folder(sourceFolder.Name, destinationFolder);
                    destinationFolder.AddEntity(newFolder);
                    CopyFolderContents(sourceFolder, newFolder);
                    Console.WriteLine($"Copied folder '{sourceFolder.Name}' to '{destinationFolder.GetFullPath()}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying entity: {ex.Message}");
            }
        }

        public void CopyFolderContents(Folder source, Folder destination)
        {
            string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destination.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(destPhysicalPath);
            foreach (var entity in source.Contents)
            {
                if (entity is File file)
                {
                    File newFile = new File(file.Name, destination, file.GetContent());
                    destination.AddEntity(newFile);
                }
                else if (entity is Folder folder)
                {
                    string subFolderPath = Path.Combine(destPhysicalPath, folder.Name);
                    Directory.CreateDirectory(subFolderPath);
                    Folder newSubFolder = new Folder(folder.Name, destination);
                    destination.AddEntity(newSubFolder);
                    CopyFolderContents(folder, newSubFolder);
                }
            }
        }

        public void NavigateTo(string path)
        {
            try
            {
                FileSystemEntity targetEntity = GetEntityFromPath(path);
                if (targetEntity == null) { Console.WriteLine("Error: Path not found."); return; }

                if (targetEntity is Folder targetFolder)
                {
                    CurrentFolder = targetFolder;
                    Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}");
                }
                else
                {
                    Console.WriteLine("Error: Cannot navigate to a file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating: {ex.Message}");
            }
        }

        public void ListContents(string path = null, bool detailed = false)
        {
            try
            {
                Folder folderToList = CurrentFolder;
                if (!string.IsNullOrEmpty(path))
                {
                    folderToList = GetEntityFromPath(path) as Folder;
                    if (folderToList == null)
                    {
                        Console.WriteLine("Error: Folder not found / Path pointing to a file instead of a folder.");
                        return;
                    }
                }
                folderToList.ListContents(detailed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void ViewFile(string path)
        {
            try
            {
                File file = GetEntityFromPath(path) as File;
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; }
                TextEditor.ViewText(file.GetContent());
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing file: {ex.Message}");
            }
        }

        public void EditFile(string path)
        {
            try
            {
                File file = GetEntityFromPath(path) as File;
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; }

                string editedContent = TextEditor.EditText(file.GetContent());
                file.Edit(editedContent);
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void SearchFiles(string searchTerm)
        {
            Console.WriteLine($"Searching for '{searchTerm}' ...");
            List<FileSystemEntity> results = new List<FileSystemEntity>();
            SearchRecursive(RootFolder, searchTerm, results);

            if (!results.Any())
            {
                Console.WriteLine("No matching files found.");
                return;
            }

            Console.WriteLine("\n--- Search Results ---");
            foreach (var entity in results)
            {
                string type = entity is File ? "File" : "Folder";
                Console.WriteLine($"- {type}: {entity.GetFullPath()} (Size: {entity.GetSize()} bytes, Created: {entity.CreationDate.ToShortDateString()})");
            }
            Console.WriteLine("----------------------");
        }

        private void SearchRecursive(Folder currentFolder, string searchTerm, List<FileSystemEntity> results)
        {
            foreach (var entity in currentFolder.Contents)
            {
                bool match = entity.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

                if (match)
                {
                    results.Add(entity);
                }

                if (entity is Folder subFolder)
                {
                    SearchRecursive(subFolder, searchTerm, results);
                }
            }
        }

        public void DisplayHistory()
        {
            Console.WriteLine("\n--- File Access History ---");
            List<string> history = _historyLogger.GetHistory();
            if (!history.Any())
            {
                Console.WriteLine("No access history found.");
                return;
            }
            foreach (var entry in history)
            {
                Console.WriteLine(entry);
            }
            Console.WriteLine("---------------------------");
        }

        public void DisplayHelp()
        {
            Console.WriteLine("\n--- Available Commands ------------------------------------------------------------------------------------------------");
            Console.WriteLine("  mkdir <folder_name>                 - Create a new folder.");
            Console.WriteLine("  touch <file_name> [content]         - Create a new file with optional content.");
            Console.WriteLine("  rm <path>                           - Delete a file or folder.");
            Console.WriteLine("  mv <source_path> <destination_path> - Move a file or folder.");
            Console.WriteLine("  cp <source_path> <destination_path> - Copy a file or folder.");
            Console.WriteLine("  rename <path> <new_name>            - Rename a file or folder.");
            Console.WriteLine("  cd <path>                           - Change directory. Use '..' for parent, '.' for current.");
            Console.WriteLine("  ls [-l] <path>                      - List contents of current or specified folder. Use '-l' for detailed view.");
            Console.WriteLine("  cat <file_path>                     - View content of a file.");
            Console.WriteLine("  nano <file_path>                    - Edit content of a file (interactive editor).");
            Console.WriteLine("  search <term>                       - Search files.");
            Console.WriteLine("  history                             - View file access history.");
            Console.WriteLine("  pwd                                 - Print working directory.");
            Console.WriteLine("  help                                - Display this help message.");
            Console.WriteLine("  exit                                - Exit the file system.");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
        }

        public void PrintWorkingDirectory()
        {
            Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}");
        }
    }

    public class FileSystemEntityConverter : JsonConverter<FileSystemEntity>
    {
        public override FileSystemEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                if (jsonDoc.RootElement.TryGetProperty("contents", out _))
                {
                    return jsonDoc.RootElement.Deserialize<Folder>(options);
                }
                else
                {
                    return jsonDoc.RootElement.Deserialize<File>(options);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, FileSystemEntity value, JsonSerializerOptions options)
        {
            if (value is File file)
            {
                JsonSerializer.Serialize(writer, file, options);
            }
            else if (value is Folder folder)
            {
                JsonSerializer.Serialize(writer, folder, options);
            }
        }
    }
}