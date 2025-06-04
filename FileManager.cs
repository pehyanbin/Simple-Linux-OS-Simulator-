using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace FileStorageSystem
{
    public class FileManager
    {
        private Folder _rootFolder;
        private Folder _currentFolder;
        private HistoryLogger _historyLogger;
        private string _baseDirectory;

        public Folder RootFolder
        {
            get { return _rootFolder; }
            set { _rootFolder = value; }
        }

        public Folder CurrentFolder
        {
            get { return _currentFolder; }
            set { _currentFolder = value; }
        }

        public HistoryLogger Historylogger
        {
            get { return _historyLogger; }
            set { _historyLogger = value; }
        }

        public FileManager()
        {
            _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
            RootFolder = new Folder("root", null);
            CurrentFolder = RootFolder;
            _historyLogger = new HistoryLogger();
            LoadPhysicalFileSystem(RootFolder, Path.Combine(_baseDirectory, "root"));
            Console.WriteLine("File System Initialized. Type 'help' for commands.");
        }

        private void LoadPhysicalFileSystem(Folder folder, string physicalPath)
        {
            try
            {
                // Ensure the folder's physical directory exists
                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                // Load directories
                foreach (var dir in Directory.GetDirectories(physicalPath))
                {
                    string dirName = Path.GetFileName(dir);
                    if (folder.GetEntity(dirName) == null)
                    {
                        Folder newFolder = new Folder(dirName, folder);
                        folder.AddEntity(newFolder);
                        // Recursively load subdirectories
                        LoadPhysicalFileSystem(newFolder, Path.Combine(physicalPath, dirName));
                    }
                }

                // Load files
                foreach (var file in Directory.GetFiles(physicalPath))
                {
                    string fileName = Path.GetFileName(file);
                    if (folder.GetEntity(fileName) == null)
                    {
                        string content = System.IO.File.ReadAllText(file);
                        File newFile = new File(fileName, folder, content);
                        folder.AddEntity(newFile);
                        // Update file metadata from physical file
                        newFile.CreationDate = System.IO.File.GetCreationTime(file);
                        newFile.LastModifiedDate = System.IO.File.GetLastWriteTime(file);
                        newFile.LastAccessedDate = System.IO.File.GetLastAccessTime(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading physical file system: {ex.Message}");
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

                string oldPath = entity.GetFullPath();
                string oldName = entity.Name;
                entity.Rename(newName);
                string newPath = entity.GetFullPath();
                if (entity is Folder && Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                }
                else if (entity is File && System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Move(oldPath, newPath);
                }
                Console.WriteLine($"'{oldName}' renamed to '{newName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

                string sourceFullPath = sourceEntity.GetFullPath();
                string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceEntity.Name);

                sourceEntity.ParentFolder.RemoveEntity(sourceEntity);
                destinationFolder.AddEntity(sourceEntity);

                if (sourceEntity is Folder && Directory.Exists(sourceFullPath))
                {
                    Directory.Move(sourceFullPath, destinationFullPath);
                }
                else if (sourceEntity is File && System.IO.File.Exists(sourceFullPath))
                {
                    System.IO.File.Move(sourceFullPath, destinationFullPath);
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

                if (sourceEntity is File sourceFile)
                {
                    File newFile = new File(sourceFile.Name, destinationFolder, sourceFile.Content);
                    destinationFolder.AddEntity(newFile);
                    string sourceFullPath = sourceFile.GetFullPath();
                    string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceFile.Name);
                    if (System.IO.File.Exists(sourceFullPath))
                    {
                        System.IO.File.Copy(sourceFullPath, destinationFullPath);
                    }
                    Console.WriteLine($"Copied file '{sourceFile.Name}' to '{destinationFolder.GetFullPath()}'.");
                }
                else if (sourceEntity is Folder sourceFolder)
                {
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
            foreach (var entity in source.Contents)
            {
                if (entity is File file)
                {
                    File newFile = new File(file.Name, destination, file.Content);
                    destination.AddEntity(newFile);
                    string sourcePath = file.GetFullPath();
                    string destPath = newFile.GetFullPath();
                    if (System.IO.File.Exists(sourcePath))
                    {
                        System.IO.File.Copy(sourcePath, destPath);
                    }
                }
                else if (entity is Folder folder)
                {
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
                        Console.WriteLine("Error: Folder not found / Path pointing to a folder instead of a file.");
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
                TextEditor.ViewText(file.Content);
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

                string editedContent = TextEditor.EditText(file.Content);
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
                bool match = entity.Name.Contains(searchTerm);

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
}