using System; // Provides fundamental classes and base types
using System.IO; // Provides types for reading and writing files and data streams
using System.Linq; // Provides classes and methods for LINQ
using System.Text.Json; // Provides functionality for serializing and deserializing JSON
using System.Text.Json.Serialization; // Provides attributes for controlling JSON serialization
using System.Collections.Generic; // Provides interfaces and classes that define generic collections
using testonly.NewFolder; // Imports the namespace containing FileSystemEntity, Folder, File, HistoryLogger, and TextEditor

namespace FileStorageSystem // Declares the namespace for the file storage system
{
    public class FileManager // Manages the file system operations
    {
        private Folder _rootFolder; // Private field for the root folder of the file system
        private Folder _currentFolder; // Private field for the current working folder
        private HistoryLogger _historyLogger; // Private field for logging file access history
        // Static and readonly string for the path where file system metadata is stored
        private static readonly string StorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_system.json");

        [JsonPropertyName("rootFolder")] // Maps the 'RootFolder' property to a JSON property named "rootFolder"
        public Folder RootFolder // Public property for the root folder
        {
            get => _rootFolder; // Getter for _rootFolder
            set => _rootFolder = value; // Setter for _rootFolder
        }

        [JsonIgnore] // Instructs the JSON serializer to ignore this property
        public Folder CurrentFolder // Public property for the current working folder
        {
            get => _currentFolder; // Getter for _currentFolder
            set => _currentFolder = value; // Setter for _currentFolder
        }

        [JsonPropertyName("currentFolderPath")] // Maps the 'CurrentFolderPath' property to a JSON property named "currentFolderPath"
        public string CurrentFolderPath { get; set; } // Stores the full path of the current folder for serialization

        [JsonIgnore] // Instructs the JSON serializer to ignore this property
        public HistoryLogger Historylogger // Public property for the history logger
        {
            get => _historyLogger; // Getter for _historyLogger
            set => _historyLogger = value; // Setter for _historyLogger
        }

        public FileManager() // Constructor for a new FileManager (when no existing system is loaded)
        {
            RootFolder = new Folder("root", null); // Initializes the root folder
            CurrentFolder = RootFolder; // Sets the current folder to the root
            _historyLogger = new HistoryLogger(); // Initializes the history logger
            CurrentFolderPath = RootFolder.GetFullPath(); // Stores the full path of the root folder
            // Creates the physical "root" directory in the application's base directory
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "root"));
            Console.WriteLine("File System Initialized. Type 'help' for commands."); // Welcome message
        }

        [JsonConstructor] // Specifies a constructor to be used by the JSON deserializer
        public FileManager(Folder rootFolder, string currentFolderPath) // Constructor for loading an existing FileManager
        {
            RootFolder = rootFolder; // Sets the root folder from the loaded data
            _historyLogger = new HistoryLogger(); // Initializes the history logger
            CurrentFolderPath = currentFolderPath; // Sets the current folder path from the loaded data
            RebuildParentReferences(RootFolder, null); // Rebuilds parent references after deserialization
            // Sets the current folder based on the loaded path, defaulting to RootFolder if path is empty or entity not found
            CurrentFolder = string.IsNullOrEmpty(currentFolderPath) ? RootFolder : GetEntityFromPath(currentFolderPath) as Folder ?? RootFolder;
            EnsureDirectoriesExist(RootFolder); // Ensures all physical directories for the loaded file system exist
            Console.WriteLine("File System Initialized. Type 'help' for commands."); // Welcome message
        }

        private void EnsureDirectoriesExist(Folder folder) // Recursively ensures physical directories exist for all folders
        {
            // Constructs the physical path for the current folder
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(folderPath); // Creates the directory if it doesn't exist
            foreach (var entity in folder.Contents) // Iterates through contents of the folder
            {
                if (entity is Folder subFolder) // If the entity is a subfolder
                {
                    EnsureDirectoriesExist(subFolder); // Recursively call for subfolders
                }
                else if (entity is File file) // If the entity is a file
                {
                    file.UpdateFilePath(); // Updates the physical file path (ensuring it's correct after loading)
                }
            }
        }

        public static FileManager Load() // Static method to load the file system from a JSON file
        {
            try
            {
                if (System.IO.File.Exists(StorageFilePath)) // Checks if the storage file exists
                {
                    string json = System.IO.File.ReadAllText(StorageFilePath); // Reads all text from the storage file
                    var options = new JsonSerializerOptions // Configures JSON deserialization options
                    {
                        Converters = { new FileSystemEntityConverter() } // Adds a custom converter for FileSystemEntity
                    };
                    return JsonSerializer.Deserialize<FileManager>(json, options); // Deserializes the JSON into a FileManager object
                }
            }
            catch (Exception ex) // Catches any exceptions during loading
            {
                Console.WriteLine($"Error loading file system: {ex.Message}. Initializing new file system."); // Prints error and indicates new system initialization
            }
            return new FileManager(); // Returns a new FileManager if loading fails or file doesn't exist
        }

        public void Save() // Saves the current state of the file system to a JSON file
        {
            try
            {
                CurrentFolderPath = CurrentFolder.GetFullPath(); // Updates CurrentFolderPath before saving
                var options = new JsonSerializerOptions // Configures JSON serialization options
                {
                    WriteIndented = true, // Enables pretty printing (indented JSON)
                    Converters = { new FileSystemEntityConverter() } // Adds a custom converter for FileSystemEntity
                };
                string json = JsonSerializer.Serialize(this, options); // Serializes the FileManager object to JSON
                System.IO.File.WriteAllText(StorageFilePath, json); // Writes the JSON string to the storage file
                Console.WriteLine("File system metadata saved successfully."); // Confirmation message
            }
            catch (Exception ex) // Catches any exceptions during saving
            {
                Console.WriteLine($"Error saving file system: {ex.Message}"); // Prints error message
            }
        }

        private void RebuildParentReferences(Folder folder, Folder parent) // Recursively rebuilds parent references after deserialization
        {
            folder.ParentFolder = parent; // Sets the parent folder for the current folder
            foreach (var entity in folder.Contents) // Iterates through contents of the folder
            {
                entity.ParentFolder = folder; // Sets the parent folder for each entity
                if (entity is Folder subFolder) // If the entity is a subfolder
                {
                    RebuildParentReferences(subFolder, folder); // Recursively call for subfolders
                }
                else if (entity is File file) // If the entity is a file
                {
                    file.UpdateFilePath(); // Updates the physical file path
                }
            }
        }

        public FileSystemEntity GetEntityFromPath(string path) // Retrieves a file system entity given its path
        {
            Folder startingFolder = CurrentFolder; // Starts search from current folder by default
            if (path.StartsWith("/")) // If path starts with '/', it's an absolute path
            {
                startingFolder = RootFolder; // Start search from the root folder
                path = path.Substring(1); // Remove the leading '/'
            }

            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries); // Splits the path into individual parts
            FileSystemEntity currentEntity = startingFolder; // Current entity being traversed

            if (pathParts.Length == 0 && path == "/") return RootFolder; // Special case for root path

            foreach (string part in pathParts) // Iterates through each part of the path
            {
                if (currentEntity is Folder folder) // If the current entity is a folder
                {
                    if (part == ".") continue; // Ignore '.' (current directory)
                    if (part == "..") // If part is '..' (parent directory)
                    {
                        if (folder.ParentFolder != null) // If a parent exists
                        {
                            currentEntity = folder.ParentFolder; // Move to the parent folder
                        }
                        else
                        {
                            return null; // No parent, path is invalid
                        }
                    }
                    else
                    {
                        currentEntity = folder.GetEntity(part); // Get the entity by name from the current folder's contents
                        if (currentEntity == null) // If entity not found
                        {
                            return null; // Path is invalid
                        }
                    }
                }
                else // If current entity is not a folder (i.e., it's a file but path continues)
                {
                    return null; // Path is invalid
                }
            }
            return currentEntity; // Returns the found entity
        }

        public Folder GetParentFolderFromPath(string path) // Retrieves the parent folder of an entity given its path
        {
            string directoryPath = Path.GetDirectoryName(path); // Gets the directory part of the path
            if (string.IsNullOrEmpty(directoryPath)) // If no directory part (i.e., entity is in current folder)
            {
                return CurrentFolder; // Return the current folder as parent
            }
            return GetEntityFromPath(directoryPath) as Folder; // Get the parent folder entity from the directory path
        }

        public string GetEntityNameFromPath(string path) // Retrieves the entity name from a given path
        {
            return Path.GetFileName(path); // Returns the file or directory name from the end of the path
        }

        public void CreateFile(string path, string content = "") // Creates a new file
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Gets the parent folder for the new file
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; } // Error if parent not found

                string fileName = GetEntityNameFromPath(path); // Gets the file name from the path
                if (string.IsNullOrEmpty(fileName)) { Console.WriteLine("Error: File name cannot be empty."); return; } // Error if file name is empty

                if (parent.GetEntity(fileName) != null) // Checks if a file with the same name already exists in the parent
                {
                    Console.WriteLine($"Error: File '{fileName}' already exists in '{parent.GetFullPath()}'."); // Error if file exists
                    return;
                }

                // Constructs the physical path for the parent directory
                string physicalParentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, parent.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                Directory.CreateDirectory(physicalParentPath); // Ensures the physical parent directory exists

                File newFile = new File(fileName, parent, content); // Creates a new File object
                parent.AddEntity(newFile); // Adds the new file to the parent folder's contents
                Console.WriteLine($"File '{newFile.Name}' created in '{parent.GetFullPath()}'."); // Confirmation message
                _historyLogger.LogAccess(newFile.GetFullPath(), DateTime.Now); // Logs the file creation as an access event
            }
            catch (Exception ex) // Catches any exceptions during file creation
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        public void CreateFolder(string path) // Creates a new folder
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Gets the parent folder for the new folder
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; } // Error if parent not found

                string folderName = GetEntityNameFromPath(path); // Gets the folder name from the path
                if (string.IsNullOrEmpty(folderName)) { Console.WriteLine("Error: Folder name cannot be empty."); return; } // Error if folder name is empty

                if (parent.GetEntity(folderName) != null) // Checks if a folder with the same name already exists in the parent
                {
                    Console.WriteLine($"Error: Folder '{folderName}' already exists in '{parent.GetFullPath()}'."); // Error if folder exists
                    return;
                }

                // Constructs the physical path for the parent directory
                string physicalParentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, parent.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                string newFolderPath = Path.Combine(physicalParentPath, folderName); // Constructs the physical path for the new folder
                Directory.CreateDirectory(newFolderPath); // Creates the physical directory

                Folder newFolder = new Folder(folderName, parent); // Creates a new Folder object
                parent.AddEntity(newFolder); // Adds the new folder to the parent folder's contents
                Console.WriteLine($"Folder '{newFolder.Name}' created in '{parent.GetFullPath()}'."); // Confirmation message
            }
            catch (Exception ex) // Catches any exceptions during folder creation
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        public void DeleteEntity(string path) // Deletes a file or folder
        {
            try
            {
                FileSystemEntity entityToDelete = GetEntityFromPath(path); // Gets the entity to delete
                if (entityToDelete == null) { Console.WriteLine("Error: File or folder not found."); return; } // Error if entity not found

                if (entityToDelete == RootFolder) // Prevents deletion of the root folder
                {
                    Console.WriteLine("Error: Cannot delete the root folder.");
                    return;
                }

                if (entityToDelete.ParentFolder != null) // Ensures the entity has a parent (not root)
                {
                    if (entityToDelete is File file) // If it's a file
                    {
                        file.DeletePhysicalFile(); // Deletes the physical file
                    }
                    else if (entityToDelete is Folder folder) // If it's a folder
                    {
                        // Constructs the physical path for the folder
                        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (Directory.Exists(folderPath)) // Checks if the physical directory exists
                        {
                            Directory.Delete(folderPath, true); // Deletes the physical directory and its contents recursively
                        }
                    }
                    entityToDelete.ParentFolder.RemoveEntity(entityToDelete); // Removes the entity from its parent's contents
                    Console.WriteLine($"'{entityToDelete.Name}' deleted."); // Confirmation message
                }
                else // Should not happen for non-root entities
                {
                    Console.WriteLine("Error: Cannot delete entity without a parent folder (should not happen for non-root).");
                }
            }
            catch (Exception ex) // Catches any exceptions during deletion
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        public void RenameEntity(string path, string newName) // Renames a file or folder
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path); // Gets the entity to rename
                if (entity == null) { Console.WriteLine("Error: File or folder not found."); return; } // Error if entity not found

                if (entity == RootFolder) // Prevents renaming the root folder
                {
                    Console.WriteLine("Error: Cannot rename the root folder.");
                    return;
                }

                string oldName = entity.Name; // Stores the old name for the message
                entity.Rename(newName); // Renames the entity in the logical file system
                if (entity is File file) // If it's a file
                {
                    file.UpdateFilePath(); // Updates the physical file path to reflect the new name
                }
                else if (entity is Folder folder) // If it's a folder
                {
                    // Constructs old and new physical paths
                    string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                    string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);
                    if (Directory.Exists(oldPath)) // Checks if the old physical directory exists
                    {
                        Directory.Move(oldPath, newPath); // Moves (renames) the physical directory
                    }
                    UpdateFolderFilePaths(folder); // Recursively updates file paths within the renamed folder
                }
                Console.WriteLine($"'{oldName}' renamed to '{newName}'."); // Confirmation message
            }
            catch (Exception ex) // Catches any exceptions during renaming
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        private void UpdateFolderFilePaths(Folder folder) // Recursively updates physical file paths within a folder (used after renaming a parent folder)
        {
            foreach (var entity in folder.Contents) // Iterates through contents
            {
                if (entity is File file) // If it's a file
                {
                    file.UpdateFilePath(); // Updates its physical file path
                }
                else if (entity is Folder subFolder) // If it's a subfolder
                {
                    UpdateFolderFilePaths(subFolder); // Recursively call for subfolders
                }
            }
        }

        public void MoveEntity(string sourcePath, string destinationPath) // Moves a file or folder
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Gets the source entity
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; } // Error if source not found

                if (sourceEntity == RootFolder) // Prevents moving the root folder
                {
                    Console.WriteLine("Error: Cannot move the root folder.");
                    return;
                }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Gets the destination folder
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; } // Error if destination is not a folder or not found

                if (destinationFolder.GetEntity(sourceEntity.Name) != null) // Checks if an entity with the same name exists in destination
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder."); // Error if duplicate name
                    return;
                }

                // Constructs source and destination physical paths
                string sourcePhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sourceEntity.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
                string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFolder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()), sourceEntity.Name);

                if (sourceEntity is File && System.IO.File.Exists(sourcePhysicalPath)) // If source is a file and exists physically
                {
                    System.IO.File.Move(sourcePhysicalPath, destPhysicalPath); // Moves the physical file
                }
                else if (sourceEntity is Folder && Directory.Exists(sourcePhysicalPath)) // If source is a folder and exists physically
                {
                    Directory.Move(sourcePhysicalPath, destPhysicalPath); // Moves the physical folder
                }

                sourceEntity.ParentFolder.RemoveEntity(sourceEntity); // Removes the entity from its original parent
                destinationFolder.AddEntity(sourceEntity); // Adds the entity to the new destination folder
                if (sourceEntity is File file) // If it's a file
                {
                    file.UpdateFilePath(); // Updates the physical file path
                }
                else if (sourceEntity is Folder folder) // If it's a folder
                {
                    UpdateFolderFilePaths(folder); // Recursively updates file paths within the moved folder
                }
                Console.WriteLine($"Moved '{sourceEntity.Name}' from '{sourceEntity.ParentFolder.GetFullPath()}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message
            }
            catch (Exception ex) // Catches any exceptions during moving
            {
                Console.WriteLine($"Error moving entity: {ex.Message}"); // Prints error message
            }
        }

        public void CopyEntity(string sourcePath, string destinationPath) // Copies a file or folder
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Gets the source entity
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; } // Error if source not found

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Gets the destination folder
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; } // Error if destination is not a folder or not found

                if (destinationFolder.GetEntity(sourceEntity.Name) != null) // Checks if an entity with the same name exists in destination
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder."); // Error if duplicate name
                    return;
                }

                // Constructs the destination physical path
                string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destinationFolder.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()), sourceEntity.Name);
                if (sourceEntity is File sourceFile) // If source is a file
                {
                    // Creates a new File object with content from the source file
                    File newFile = new File(sourceFile.Name, destinationFolder, sourceFile.GetContent());
                    destinationFolder.AddEntity(newFile); // Adds the new file to the destination folder
                    Console.WriteLine($"Copied file '{sourceFile.Name}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message
                }
                else if (sourceEntity is Folder sourceFolder) // If source is a folder
                {
                    Directory.CreateDirectory(destPhysicalPath); // Creates the physical directory for the new folder
                    Folder newFolder = new Folder(sourceFolder.Name, destinationFolder); // Creates a new Folder object
                    destinationFolder.AddEntity(newFolder); // Adds the new folder to the destination folder
                    CopyFolderContents(sourceFolder, newFolder); // Recursively copies contents of the source folder to the new folder
                    Console.WriteLine($"Copied folder '{sourceFolder.Name}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message
                }
            }
            catch (Exception ex) // Catches any exceptions during copying
            {
                Console.WriteLine($"Error copying entity: {ex.Message}"); // Prints error message
            }
        }

        public void CopyFolderContents(Folder source, Folder destination) // Recursively copies contents of a source folder to a destination folder
        {
            // Constructs the destination physical path for the folder
            string destPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destination.GetFullPath().Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(destPhysicalPath); // Ensures the physical directory exists
            foreach (var entity in source.Contents) // Iterates through contents of the source folder
            {
                if (entity is File file) // If it's a file
                {
                    File newFile = new File(file.Name, destination, file.GetContent()); // Creates a new file with content
                    destination.AddEntity(newFile); // Adds to destination
                }
                else if (entity is Folder folder) // If it's a folder
                {
                    string subFolderPath = Path.Combine(destPhysicalPath, folder.Name); // Constructs physical path for subfolder
                    Directory.CreateDirectory(subFolderPath); // Creates physical subfolder
                    Folder newSubFolder = new Folder(folder.Name, destination); // Creates new Folder object
                    destination.AddEntity(newSubFolder); // Adds to destination
                    CopyFolderContents(folder, newSubFolder); // Recursively copies subfolder contents
                }
            }
        }

        public void NavigateTo(string path) // Navigates to a specified folder
        {
            try
            {
                FileSystemEntity targetEntity = GetEntityFromPath(path); // Gets the target entity
                if (targetEntity == null) { Console.WriteLine("Error: Path not found."); return; } // Error if path not found

                if (targetEntity is Folder targetFolder) // If the target is a folder
                {
                    CurrentFolder = targetFolder; // Sets current folder to the target folder
                    Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}"); // Confirmation message
                }
                else // If the target is a file
                {
                    Console.WriteLine("Error: Cannot navigate to a file."); // Error message
                }
            }
            catch (Exception ex) // Catches any exceptions during navigation
            {
                Console.WriteLine($"Error navigating: {ex.Message}"); // Prints error message
            }
        }

        public void ListContents(string path = null, bool detailed = false) // Lists the contents of a specified or current folder
        {
            try
            {
                Folder folderToList = CurrentFolder; // Defaults to current folder
                if (!string.IsNullOrEmpty(path)) // If a path is provided
                {
                    folderToList = GetEntityFromPath(path) as Folder; // Gets the folder from the path
                    if (folderToList == null) // If the path does not point to a folder
                    {
                        Console.WriteLine("Error: Folder not found / Path pointing to a file instead of a folder."); // Error message
                        return;
                    }
                }
                folderToList.ListContents(detailed); // Calls the folder's ListContents method
            }
            catch (Exception ex) // Catches any exceptions during listing
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        public void ViewFile(string path) // Views the content of a file
        {
            try
            {
                File file = GetEntityFromPath(path) as File; // Gets the file from the path
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; } // Error if not a file or not found
                TextEditor.ViewText(file.GetContent()); // Uses TextEditor to view file content
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Logs file access
            }
            catch (Exception ex) // Catches any exceptions during viewing
            {
                Console.WriteLine($"Error viewing file: {ex.Message}"); // Prints error message
            }
        }

        public void EditFile(string path) // Edits the content of a file
        {
            try
            {
                File file = GetEntityFromPath(path) as File; // Gets the file from the path
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; } // Error if not a file or not found

                string editedContent = TextEditor.EditText(file.GetContent()); // Opens the interactive text editor with current content
                file.Edit(editedContent); // Updates the file's content with the edited content
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Logs file access
            }
            catch (Exception ex) // Catches any exceptions during editing
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints error message
            }
        }

        public void SearchFiles(string searchTerm) // Searches for files and folders containing a search term in their name
        {
            Console.WriteLine($"Searching for '{searchTerm}' ..."); // Prints search query
            List<FileSystemEntity> results = new List<FileSystemEntity>(); // List to store search results
            SearchRecursive(RootFolder, searchTerm, results); // Starts recursive search from the root folder

            if (!results.Any()) // If no results found
            {
                Console.WriteLine("No matching files found."); // Prints no results message
                return;
            }

            Console.WriteLine("\n--- Search Results ---"); // Header for search results
            foreach (var entity in results) // Iterates through search results
            {
                string type = entity is File ? "File" : "Folder"; // Determines entity type
                // Prints search result details
                Console.WriteLine($"- {type}: {entity.GetFullPath()} (Size: {entity.GetSize()} bytes, Created: {entity.CreationDate.ToShortDateString()})");
            }
            Console.WriteLine("----------------------"); // Footer for search results
        }

        private void SearchRecursive(Folder currentFolder, string searchTerm, List<FileSystemEntity> results) // Recursively searches for entities
        {
            foreach (var entity in currentFolder.Contents) // Iterates through contents of the current folder
            {
                bool match = entity.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase); // Checks if entity name contains search term

                if (match) // If there's a match
                {
                    results.Add(entity); // Add entity to results
                }

                if (entity is Folder subFolder) // If entity is a subfolder
                {
                    SearchRecursive(subFolder, searchTerm, results); // Recursively call for subfolder
                }
            }
        }

        public void DisplayHistory() // Displays the file access history
        {
            Console.WriteLine("\n--- File Access History ---"); // Header for history
            List<string> history = _historyLogger.GetHistory(); // Gets history from the logger
            if (!history.Any()) // If no history found
            {
                Console.WriteLine("No access history found."); // Prints no history message
                return;
            }
            foreach (var entry in history) // Iterates through history entries
            {
                Console.WriteLine(entry); // Prints each history entry
            }
            Console.WriteLine("---------------------------"); // Footer for history
        }

        public void DisplayHelp() // Displays a list of available commands and their usage
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

        public void PrintWorkingDirectory() // Prints the full path of the current working directory
        {
            Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}");
        }
    }

    public class FileSystemEntityConverter : JsonConverter<FileSystemEntity> // Custom JSON converter for FileSystemEntity
    {
        public override FileSystemEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) // Deserializes a FileSystemEntity
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader)) // Parses the JSON element
            {
                if (jsonDoc.RootElement.TryGetProperty("contents", out _)) // Checks if the JSON element has a "contents" property (indicates a Folder)
                {
                    return jsonDoc.RootElement.Deserialize<Folder>(options); // Deserializes as a Folder
                }
                else // Otherwise, it's a File
                {
                    return jsonDoc.RootElement.Deserialize<File>(options); // Deserializes as a File
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, FileSystemEntity value, JsonSerializerOptions options) // Serializes a FileSystemEntity
        {
            if (value is File file) // If the value is a File
            {
                JsonSerializer.Serialize(writer, file, options); // Serialize it as a File
            }
            else if (value is Folder folder) // If the value is a Folder
            {
                JsonSerializer.Serialize(writer, folder, options); // Serialize it as a Folder
            }
        }
    }
}