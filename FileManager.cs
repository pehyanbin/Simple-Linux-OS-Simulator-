using System; // Imports the System namespace, providing fundamental classes and base types.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.
using System.IO; // Imports the System.IO namespace, for input/output operations like file and directory manipulation.
using System.Collections.Generic; // Imports the System.Collections.Generic namespace, for generic collection types like List.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public class FileManager // Declares the public class FileManager, responsible for managing file system operations.
    {
        private Folder _rootFolder; // Declares a private field to store the root folder of the file system.
        private Folder _currentFolder; // Declares a private field to store the current working folder.
        private HistoryLogger _historyLogger; // Declares a private field for logging file access history.
        private string _baseDirectory; // Declares a private field to store the base physical directory for storage.

        public Folder RootFolder // Public property to get or set the root folder.
        {
            get { return _rootFolder; } // Getter for RootFolder.
            set { _rootFolder = value; } // Setter for RootFolder.
        }

        public Folder CurrentFolder // Public property to get or set the current folder.
        {
            get { return _currentFolder; } // Getter for CurrentFolder.
            set { _currentFolder = value; } // Setter for CurrentFolder.
        }

        public HistoryLogger Historylogger // Public property to get or set the history logger.
        {
            get { return _historyLogger; } // Getter for Historylogger.
            set { _historyLogger = value; } // Setter for Historylogger.
        }

        public FileManager() // Constructor for the FileManager class.
        {
            // Combines the application's base directory with "FileStorage" to create the absolute base directory path.
            _baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            if (!Directory.Exists(_baseDirectory)) // Checks if the base directory does not exist.
            {
                Directory.CreateDirectory(_baseDirectory); // Creates the base directory if it doesn't exist.
            }
            RootFolder = new Folder("root", null); // Initializes the root folder with the name "root" and no parent.
            CurrentFolder = RootFolder; // Sets the current folder to the root folder.
            _historyLogger = new HistoryLogger(); // Initializes a new instance of the HistoryLogger.
            // Loads the physical file system into the in-memory structure, starting from the root.
            LoadPhysicalFileSystem(RootFolder, Path.Combine(_baseDirectory, "root"));
            Console.WriteLine("File System Initialized. Type 'help' for commands."); // Informs the user that the file system is initialized.
        }

        // Recursively loads the physical file system (directories and files) into the in-memory folder structure.
        private void LoadPhysicalFileSystem(Folder folder, string physicalPath)
        {
            try
            {
                // Ensure the folder's physical directory exists
                if (!Directory.Exists(physicalPath)) // Checks if the physical directory for the current folder exists.
                {
                    Directory.CreateDirectory(physicalPath); // Creates the directory if it doesn't exist.
                }

                // Load directories
                foreach (var dir in Directory.GetDirectories(physicalPath)) // Iterates through all subdirectories in the physical path.
                {
                    string dirName = Path.GetFileName(dir); // Gets the name of the directory.
                    if (folder.GetEntity(dirName) == null) // Checks if a folder with the same name doesn't already exist in the in-memory structure.
                    {
                        Folder newFolder = new Folder(dirName, folder); // Creates a new in-memory Folder object.
                        folder.AddEntity(newFolder); // Adds the new folder to the current in-memory folder's contents.
                        // Recursively load subdirectories
                        LoadPhysicalFileSystem(newFolder, Path.Combine(physicalPath, dirName)); // Recursively calls itself for the new subfolder.
                    }
                }

                // Load files
                foreach (var file in Directory.GetFiles(physicalPath)) // Iterates through all files in the physical path.
                {
                    string fileName = Path.GetFileName(file); // Gets the name of the file.
                    if (folder.GetEntity(fileName) == null) // Checks if a file with the same name doesn't already exist in the in-memory structure.
                    {
                        string content = System.IO.File.ReadAllText(file); // Reads all text content from the physical file.
                        File newFile = new File(fileName, folder, content); // Creates a new in-memory File object with the content.
                        folder.AddEntity(newFile); // Adds the new file to the current in-memory folder's contents.
                        // Update file metadata from physical file
                        newFile.CreationDate = System.IO.File.GetCreationTime(file); // Sets the creation date from the physical file.
                        newFile.LastModifiedDate = System.IO.File.GetLastWriteTime(file); // Sets the last modified date from the physical file.
                        newFile.LastAccessedDate = System.IO.File.GetLastAccessTime(file); // Sets the last accessed date from the physical file.
                    }
                }
            }
            catch (Exception ex) // Catches any exceptions that occur during the loading process.
            {
                Console.WriteLine($"Error loading physical file system: {ex.Message}"); // Prints an error message to the console.
            }
        }

        // Retrieves a FileSystemEntity (file or folder) from a given path.
        public FileSystemEntity GetEntityFromPath(string path)
        {
            Folder startingFolder = CurrentFolder; // Starts the search from the current folder by default.
            if (path.StartsWith("/")) // Checks if the path is an absolute path (starts with '/').
            {
                startingFolder = RootFolder; // If absolute, start from the root folder.
                path = path.Substring(1); // Removes the leading '/' from the path.
            }

            // Splits the path into individual parts (folder/file names) using '/' as a delimiter, removing empty entries.
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            FileSystemEntity currentEntity = startingFolder; // Sets the current entity to the starting folder.

            if (pathParts.Length == 0 && path == "/") return RootFolder; // If the path is just "/", return the RootFolder.

            foreach (string part in pathParts) // Iterates through each part of the path.
            {
                if (currentEntity is Folder folder) // Checks if the current entity is a folder.
                {
                    if (part == ".") continue; // If the part is ".", it means current directory, so continue to the next part.
                    if (part == "..") // If the part is "..", it means parent directory.
                    {
                        if (folder.ParentFolder != null) // Checks if the current folder has a parent.
                        {
                            currentEntity = folder.ParentFolder; // Moves up to the parent folder.
                        }
                        else
                        {
                            return null; // If no parent (e.g., trying to 'cd ..' from root), return null.
                        }
                    }
                    else
                    {
                        currentEntity = folder.GetEntity(part); // Tries to get the entity (file or folder) with the current part's name from the current folder.
                        if (currentEntity == null) // If the entity is not found.
                        {
                            return null; // Return null.
                        }
                    }
                }
                else
                {
                    return null; // If the current entity is not a folder but the path still has parts, it's an invalid path.
                }
            }
            return currentEntity; // Returns the found FileSystemEntity.
        }

        // Retrieves the parent folder from a given path.
        public Folder GetParentFolderFromPath(string path)
        {
            string directoryPath = Path.GetDirectoryName(path); // Extracts the directory path part from the full path.
            if (string.IsNullOrEmpty(directoryPath)) // If the extracted directory path is null or empty (e.g., just a file name).
            {
                return CurrentFolder; // The parent is the current folder.
            }
            return GetEntityFromPath(directoryPath) as Folder; // Gets the entity for the directory path and casts it to a Folder.
        }

        // Retrieves the entity name (file or folder name) from a given path.
        public string GetEntityNameFromPath(string path)
        {
            return Path.GetFileName(path); // Uses Path.GetFileName to extract the name.
        }

        // Creates a new file at the specified path with optional content.
        public void CreateFile(string path, string content = "")
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Gets the parent folder where the file should be created.
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; } // Error if parent not found.

                string fileName = GetEntityNameFromPath(path); // Gets the name of the file to be created.
                if (string.IsNullOrEmpty(fileName)) { Console.WriteLine("Error: File name cannot be empty."); return; } // Error if file name is empty.

                if (parent.GetEntity(fileName) != null) // Checks if an entity with the same name already exists in the parent folder.
                {
                    Console.WriteLine($"Error: File '{fileName}' already exists in '{parent.GetFullPath()}'."); // Error if file exists.
                    return;
                }

                File newFile = new File(fileName, parent, content); // Creates a new File object.
                parent.AddEntity(newFile); // Adds the new file to the parent folder's contents.
                Console.WriteLine($"File '{newFile.Name}' created in '{parent.GetFullPath()}'."); // Confirmation message.
                _historyLogger.LogAccess(newFile.GetFullPath(), DateTime.Now); // Logs the file creation as an access event.
            }
            catch (Exception ex) // Catches any exceptions during file creation.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Creates a new folder at the specified path.
        public void CreateFolder(string path)
        {
            try
            {
                Folder parent = GetParentFolderFromPath(path); // Gets the parent folder.
                if (parent == null) { Console.WriteLine("Error: Parent directory not found."); return; } // Error if parent not found.

                string folderName = GetEntityNameFromPath(path); // Gets the name of the folder to be created.
                if (string.IsNullOrEmpty(folderName)) { Console.WriteLine("Error: Folder name cannot be empty."); return; } // Error if folder name is empty.

                if (parent.GetEntity(folderName) != null) // Checks if a folder with the same name already exists.
                {
                    Console.WriteLine($"Error: Folder '{folderName}' already exists in '{parent.GetFullPath()}'."); // Error if folder exists.
                    return;
                }

                Folder newFolder = new Folder(folderName, parent); // Creates a new Folder object.
                parent.AddEntity(newFolder); // Adds the new folder to the parent's contents.
                Console.WriteLine($"Folder '{newFolder.Name}' created in '{parent.GetFullPath()}'."); // Confirmation message.
            }
            catch (Exception ex) // Catches any exceptions during folder creation.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Deletes a file or folder at the specified path.
        public void DeleteEntity(string path)
        {
            try
            {
                FileSystemEntity entityToDelete = GetEntityFromPath(path); // Gets the entity to be deleted.
                if (entityToDelete == null) { Console.WriteLine("Error: File or folder not found."); return; } // Error if entity not found.

                if (entityToDelete == RootFolder) // Prevents deletion of the root folder.
                {
                    Console.WriteLine("Error: Cannot delete the root folder.");
                    return;
                }

                if (entityToDelete.ParentFolder != null) // Checks if the entity has a parent folder.
                {
                    entityToDelete.ParentFolder.RemoveEntity(entityToDelete); // Removes the entity from its parent folder.
                    Console.WriteLine($"'{entityToDelete.Name}' deleted."); // Confirmation message.
                }
                else
                {
                    // This case should ideally not be reached for non-root entities.
                    Console.WriteLine("Error: Cannot delete entity without a parent folder (should not happen for non-root).");
                }
            }
            catch (Exception ex) // Catches any exceptions during deletion.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Renames a file or folder.
        public void RenameEntity(string path, string newName)
        {
            try
            {
                FileSystemEntity entity = GetEntityFromPath(path); // Gets the entity to be renamed.
                if (entity == null) { Console.WriteLine("Error: File or folder not found."); return; } // Error if entity not found.

                if (entity == RootFolder) // Prevents renaming the root folder.
                {
                    Console.WriteLine("Error: Cannot rename the root folder.");
                    return;
                }

                string oldPath = entity.GetFullPath(); // Gets the old full path of the entity.
                string oldName = entity.Name; // Stores the old name.
                entity.Rename(newName); // Calls the Rename method on the entity.
                string newPath = entity.GetFullPath(); // Gets the new full path of the entity.

                // If the entity is a folder and its physical directory exists, move the physical directory.
                if (entity is Folder && Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                }
                // If the entity is a file and its physical file exists, move the physical file.
                else if (entity is File && System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Move(oldPath, newPath);
                }
                Console.WriteLine($"'{oldName}' renamed to '{newName}'."); // Confirmation message.
            }
            catch (Exception ex) // Catches any exceptions during renaming.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Moves a file or folder from a source path to a destination path.
        public void MoveEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Gets the source entity to be moved.
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; } // Error if source not found.

                if (sourceEntity == RootFolder) // Prevents moving the root folder.
                {
                    Console.WriteLine("Error: Cannot move the root folder.");
                    return;
                }

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Gets the destination folder.
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; } // Error if destination is not a folder or not found.

                // Checks if an entity with the same name already exists in the destination folder.
                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder."); // Error if entity exists.
                    return;
                }

                string sourceFullPath = sourceEntity.GetFullPath(); // Gets the full path of the source entity.
                // Constructs the full path for the destination.
                string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceEntity.Name);

                sourceEntity.ParentFolder.RemoveEntity(sourceEntity); // Removes the entity from its original parent folder.
                destinationFolder.AddEntity(sourceEntity); // Adds the entity to the new destination folder.

                // If the source is a folder and its physical directory exists, move the physical directory.
                if (sourceEntity is Folder && Directory.Exists(sourceFullPath))
                {
                    Directory.Move(sourceFullPath, destinationFullPath);
                }
                // If the source is a file and its physical file exists, move the physical file.
                else if (sourceEntity is File && System.IO.File.Exists(sourceFullPath))
                {
                    System.IO.File.Move(sourceFullPath, destinationFullPath);
                }

                Console.WriteLine($"Moved '{sourceEntity.Name}' from '{sourceEntity.ParentFolder.GetFullPath()}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message.
            }
            catch (Exception ex) // Catches any exceptions during moving.
            {
                Console.WriteLine($"Error moving entity: {ex.Message}"); // Prints the error message.
            }
        }

        // Copies a file or folder from a source path to a destination path.
        public void CopyEntity(string sourcePath, string destinationPath)
        {
            try
            {
                FileSystemEntity sourceEntity = GetEntityFromPath(sourcePath); // Gets the source entity to be copied.
                if (sourceEntity == null) { Console.WriteLine("Error: Source file or folder not found."); return; } // Error if source not found.

                Folder destinationFolder = GetEntityFromPath(destinationPath) as Folder; // Gets the destination folder.
                if (destinationFolder == null) { Console.WriteLine("Error: Destination folder not found or is a file."); return; } // Error if destination is not a folder or not found.

                // Checks if an entity with the same name already exists in the destination folder.
                if (destinationFolder.GetEntity(sourceEntity.Name) != null)
                {
                    Console.WriteLine($"Error: An entity named '{sourceEntity.Name}' already exists in the destination folder."); // Error if entity exists.
                    return;
                }

                if (sourceEntity is File sourceFile) // If the source entity is a file.
                {
                    File newFile = new File(sourceFile.Name, destinationFolder, sourceFile.Content); // Creates a new File object with the same name, parent, and content.
                    destinationFolder.AddEntity(newFile); // Adds the new file to the destination folder.
                    string sourceFullPath = sourceFile.GetFullPath(); // Gets the full path of the source file.
                    string destinationFullPath = Path.Combine(destinationFolder.GetFullPath(), sourceFile.Name); // Constructs the full path for the destination file.
                    if (System.IO.File.Exists(sourceFullPath)) // If the physical source file exists.
                    {
                        System.IO.File.Copy(sourceFullPath, destinationFullPath); // Copies the physical file.
                    }
                    Console.WriteLine($"Copied file '{sourceFile.Name}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message.
                }
                else if (sourceEntity is Folder sourceFolder) // If the source entity is a folder.
                {
                    Folder newFolder = new Folder(sourceFolder.Name, destinationFolder); // Creates a new Folder object with the same name and parent.
                    destinationFolder.AddEntity(newFolder); // Adds the new folder to the destination folder.

                    CopyFolderContents(sourceFolder, newFolder); // Recursively copies the contents of the source folder to the new folder.
                    Console.WriteLine($"Copied folder '{sourceFolder.Name}' to '{destinationFolder.GetFullPath()}'."); // Confirmation message.
                }
            }
            catch (Exception ex) // Catches any exceptions during copying.
            {
                Console.WriteLine($"Error copying entity: {ex.Message}"); // Prints the error message.
            }
        }

        // Recursively copies the contents (files and subfolders) from a source folder to a destination folder.
        public void CopyFolderContents(Folder source, Folder destination)
        {
            foreach (var entity in source.Contents) // Iterates through each entity in the source folder.
            {
                if (entity is File file) // If the entity is a file.
                {
                    File newFile = new File(file.Name, destination, file.Content); // Creates a new File object for the destination.
                    destination.AddEntity(newFile); // Adds the new file to the destination folder.
                    string sourcePath = file.GetFullPath(); // Gets the full path of the source file.
                    string destPath = newFile.GetFullPath(); // Gets the full path of the new file.
                    if (System.IO.File.Exists(sourcePath)) // If the physical source file exists.
                    {
                        System.IO.File.Copy(sourcePath, destPath); // Copies the physical file.
                    }
                }
                else if (entity is Folder folder) // If the entity is a folder.
                {
                    Folder newSubFolder = new Folder(folder.Name, destination); // Creates a new subfolder object for the destination.
                    destination.AddEntity(newSubFolder); // Adds the new subfolder to the destination folder.
                    CopyFolderContents(folder, newSubFolder); // Recursively calls itself to copy the contents of the subfolder.
                }
            }
        }

        // Navigates to a specified folder.
        public void NavigateTo(string path)
        {
            try
            {
                FileSystemEntity targetEntity = GetEntityFromPath(path); // Gets the target entity.
                if (targetEntity == null) { Console.WriteLine("Error: Path not found."); return; } // Error if path not found.

                if (targetEntity is Folder targetFolder) // If the target entity is a folder.
                {
                    CurrentFolder = targetFolder; // Sets the current folder to the target folder.
                    Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}"); // Displays the new current directory.
                }
                else
                {
                    Console.WriteLine("Error: Cannot navigate to a file."); // Error if trying to navigate to a file.
                }
            }
            catch (Exception ex) // Catches any exceptions during navigation.
            {
                Console.WriteLine($"Error navigating: {ex.Message}"); // Prints the error message.
            }
        }

        // Lists the contents of the current or specified folder.
        public void ListContents(string path = null, bool detailed = false)
        {
            try
            {
                Folder folderToList = CurrentFolder; // Defaults to the current folder.
                if (!string.IsNullOrEmpty(path)) // If a path is provided.
                {
                    folderToList = GetEntityFromPath(path) as Folder; // Gets the folder from the specified path.
                    if (folderToList == null) // If the entity at the path is not a folder or not found.
                    {
                        Console.WriteLine("Error: Folder not found / Path pointing to a file instead of a folder."); // Error message.
                        return;
                    }
                }
                folderToList.ListContents(detailed); // Calls the ListContents method on the folder object.
            }
            catch (Exception ex) // Catches any exceptions during listing.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Views the content of a specified file.
        public void ViewFile(string path)
        {
            try
            {
                File file = GetEntityFromPath(path) as File; // Gets the file from the specified path.
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; } // Error if not a file or not found.
                TextEditor.ViewText(file.Content); // Calls the ViewText method from TextEditor to display content.
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Logs the file access.
            }
            catch (Exception ex) // Catches any exceptions during viewing.
            {
                Console.WriteLine($"Error viewing file: {ex.Message}"); // Prints the error message.
            }
        }

        // Edits the content of a specified file using an interactive text editor.
        public void EditFile(string path)
        {
            try
            {
                File file = GetEntityFromPath(path) as File; // Gets the file from the specified path.
                if (file == null) { Console.WriteLine("Error: File not found / Path pointing to a folder instead of a file."); return; } // Error if not a file or not found.

                string editedContent = TextEditor.EditText(file.Content); // Calls the EditText method from TextEditor to get edited content.
                file.Edit(editedContent); // Updates the file's content with the edited content.
                _historyLogger.LogAccess(file.GetFullPath(), DateTime.Now); // Logs the file access (editing is also an access).
            }
            catch (Exception ex) // Catches any exceptions during editing.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
            }
        }

        // Searches for files and folders whose names contain the given search term.
        public void SearchFiles(string searchTerm)
        {
            Console.WriteLine($"Searching for '{searchTerm}' ..."); // Informs the user about the search.
            List<FileSystemEntity> results = new List<FileSystemEntity>(); // Creates a list to store search results.
            SearchRecursive(RootFolder, searchTerm, results); // Starts a recursive search from the root folder.

            if (!results.Any()) // If no results are found.
            {
                Console.WriteLine("No matching files found."); // Informs the user.
                return;
            }

            Console.WriteLine("\n--- Search Results ---"); // Header for search results.
            foreach (var entity in results) // Iterates through each found entity.
            {
                string type = entity is File ? "File" : "Folder"; // Determines if the entity is a file or folder.
                Console.WriteLine($"- {type}: {entity.GetFullPath()} (Size: {entity.GetSize()} bytes, Created: {entity.CreationDate.ToShortDateString()})"); // Prints details of the found entity.
            }
            Console.WriteLine("----------------------"); // Footer for search results.
        }

        // Recursively searches for entities containing the search term within a given folder and its subfolders.
        private void SearchRecursive(Folder currentFolder, string searchTerm, List<FileSystemEntity> results)
        {
            foreach (var entity in currentFolder.Contents) // Iterates through each entity in the current folder.
            {
                bool match = entity.Name.Contains(searchTerm); // Checks if the entity's name contains the search term.

                if (match) // If there's a match.
                {
                    results.Add(entity); // Adds the entity to the results list.
                }

                if (entity is Folder subFolder) // If the current entity is a subfolder.
                {
                    SearchRecursive(subFolder, searchTerm, results); // Recursively calls itself for the subfolder.
                }
            }
        }

        // Displays the file access history.
        public void DisplayHistory()
        {
            Console.WriteLine("\n--- File Access History ---"); // Header for history.
            List<string> history = _historyLogger.GetHistory(); // Retrieves the access history from the logger.
            if (!history.Any()) // If no history entries are found.
            {
                Console.WriteLine("No access history found."); // Informs the user.
                return;
            }
            foreach (var entry in history) // Iterates through each history entry.
            {
                Console.WriteLine(entry); // Prints the history entry.
            }
            Console.WriteLine("---------------------------"); // Footer for history.
        }

        // Displays a help message with available commands.
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

        // Prints the current working directory (full path of the current folder).
        public void PrintWorkingDirectory()
        {
            Console.WriteLine($"Current directory: {CurrentFolder.GetFullPath()}"); // Displays the full path.
        }
    }
}