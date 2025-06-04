using System; // Provides fundamental classes and base types
using System.IO; // Provides types for reading and writing files and data streams
using System.Linq; // Provides classes and methods for LINQ
using System.Text.Json.Serialization; // Provides attributes for controlling JSON serialization
using System.Xml.Linq; // Not directly used in the provided code, but typically for XML manipulation
using testonly.NewFolder; // Imports the namespace containing FileSystemEntity and Folder

namespace FileStorageSystem // Declares the namespace for the file storage system
{
    public class File : FileSystemEntity // Defines the File class, inheriting from FileSystemEntity
    {
        public string FilePath { get; private set; } // Gets the physical path of the file on the disk

        [JsonConstructor] // Specifies a constructor to be used by the JSON deserializer
        public File(string name, DateTime creationDate, DateTime lastModifiedDate, DateTime lastAccessedDate)
            : base(name, null) // Calls the base class constructor with name and a null parent (parent will be rebuilt later)
        {
            CreationDate = creationDate; // Sets the creation date
            LastModifiedDate = lastModifiedDate; // Sets the last modified date
            LastAccessedDate = lastAccessedDate; // Sets the last accessed date
        }

        public File(string name, Folder parent, string content = "") : base(name, parent) // Constructor for creating a new file
        {
            FilePath = Path.Combine(GetPhysicalFolderPath(), $"{name}.txt"); // Constructs the physical file path
            System.IO.File.WriteAllText(FilePath, content); // Writes the initial content to the physical file
        }

        private string GetPhysicalFolderPath() // Helper method to get the physical folder path on disk
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Gets the base directory of the application
            string folderPath = ParentFolder?.GetFullPath() ?? "root"; // Gets the full path of the logical parent folder, or "root" if no parent
            // Combines the base path with the logical folder path (replacing '/' with system-specific directory separator)
            return Path.Combine(basePath, folderPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        public string GetContent() // Gets the content of the file
        {
            LastAccessedDate = DateTime.Now; // Updates the last accessed date
            // Reads all text from the physical file if it exists, otherwise returns an empty string
            return System.IO.File.Exists(FilePath) ? System.IO.File.ReadAllText(FilePath) : "";
        }

        public void SetContent(string content) // Sets the content of the file
        {
            System.IO.File.WriteAllText(FilePath, content); // Writes the content to the physical file
            LastModifiedDate = DateTime.Now; // Updates the last modified date
        }

        public override long GetSize() // Overrides the abstract GetSize method from FileSystemEntity
        {
            if (System.IO.File.Exists(FilePath)) // Checks if the physical file exists
            {
                return new FileInfo(FilePath).Length; // Returns the size of the physical file in bytes
            }
            return 0; // Returns 0 if the file does not exist
        }

        public void Edit(string newContent) // Edits the content of the file
        {
            SetContent(newContent); // Sets the new content
            Console.WriteLine($"File '{Name}' content updated."); // Confirmation message
        }

        public void AppendContent(string contentToAppend) // Appends content to the end of the file
        {
            System.IO.File.AppendAllText(FilePath, contentToAppend); // Appends text to the physical file
            LastModifiedDate = DateTime.Now; // Updates the last modified date
            Console.WriteLine($"Content appended to '{Name}'."); // Confirmation message
        }

        public void PrependContent(string contentToPrepend) // Prepends content to the beginning of the file
        {
            string currentContent = GetContent(); // Gets the current content of the file
            SetContent(contentToPrepend + currentContent); // Sets the new content (prepended + current)
            Console.WriteLine($"Content prepended to '{Name}'."); // Confirmation message
        }

        public void InsertContent(int lineNumber, string contentToInsert) // Inserts content at a specific line number
        {
            // Splits current content into lines
            var lines = GetContent().Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count + 1) // Checks if the line number is out of range
            {
                throw new ArgumentOutOfRangeException("Line number out of range."); // Throws an exception
            }
            lines.Insert(lineNumber - 1, contentToInsert); // Inserts the content at the specified line index
            SetContent(string.Join(Environment.NewLine, lines)); // Joins lines back and sets as new content
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'."); // Confirmation message
        }

        public void DeleteLine(int lineNumber) // Deletes a specific line from the file
        {
            // Splits current content into lines
            var lines = GetContent().Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count) // Checks if the line number is out of range
            {
                throw new ArgumentOutOfRangeException("Line number out of range."); // Throws an exception
            }
            lines.RemoveAt(lineNumber - 1); // Removes the line at the specified line index
            SetContent(string.Join(Environment.NewLine, lines)); // Joins lines back and sets as new content
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'."); // Confirmation message
        }

        public void View() // Displays the content of the file to the console
        {
            LastAccessedDate = DateTime.Now; // Updates the last accessed date
            Console.WriteLine($"--- Content of {Name} ---"); // Header for content display
            Console.WriteLine(GetContent()); // Prints the file content
            Console.WriteLine("--------------------------"); // Footer for content display
        }

        public void UpdateFilePath() // Updates the physical file path after a rename or move operation
        {
            string oldPath = FilePath; // Stores the old physical path
            FilePath = Path.Combine(GetPhysicalFolderPath(), $"{Name}.txt"); // Constructs the new physical file path
            if (System.IO.File.Exists(oldPath)) // Checks if the file existed at the old path
            {
                System.IO.File.Move(oldPath, FilePath); // Moves the physical file to the new path (renames/relocates)
            }
        }

        public void DeletePhysicalFile() // Deletes the physical file from disk
        {
            if (System.IO.File.Exists(FilePath)) // Checks if the physical file exists
            {
                System.IO.File.Delete(FilePath); // Deletes the physical file
            }
        }
    }
}