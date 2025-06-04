using System; // Imports the System namespace, providing fundamental classes and base types.
using System.IO; // Imports the System.IO namespace, for input/output operations like file and directory manipulation.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.
using System.Collections.Generic; // Imports the System.Collections.Generic namespace, for generic collection types like List.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public class File : FileSystemEntity // Declares the public class File, inheriting from FileSystemEntity.
    {
        private string _content; // Private field to store the content of the file.
        public string Content // Public property for the file's content.
        {
            get
            {
                LastAccessedDate = DateTime.Now; // Updates the last accessed date whenever content is read.
                string filePath = GetFullPath(); // Gets the full physical path of the file.
                if (System.IO.File.Exists(filePath)) // Checks if the physical file exists.
                {
                    _content = System.IO.File.ReadAllText(filePath); // Reads all text from the physical file into the _content field.
                }
                return _content; // Returns the content.
            }
            set
            {
                _content = value; // Sets the private _content field.
                LastModifiedDate = DateTime.Now; // Updates the last modified date whenever content is written.
                string filePath = GetFullPath(); // Gets the full physical path of the file.
                System.IO.File.WriteAllText(filePath, _content); // Writes the content to the physical file, overwriting existing content.
            }
        }

        // Constructor for the File class.
        public File(string name, Folder parent, string content = "") : base(name, parent) // Calls the base constructor.
        {
            _content = content; // Initializes the private _content field with the provided content.
            string filePath = GetFullPath(); // Gets the full physical path for this file.

            if (!System.IO.File.Exists(filePath)) // Checks if the physical file does not exist.
            {
                // Note: There's a potential inconsistency here, it adds ".txt" to the path during creation.
                // This means the internal 'GetFullPath()' might return "root/file.txt" while the File.WriteAllText creates "root/file.txt.txt"
                System.IO.File.WriteAllText(filePath + ".txt", content);
            }
        }

        public override long GetSize() // Overrides the abstract GetSize method from FileSystemEntity.
        {
            string filePath = GetFullPath(); // Gets the full physical path of the file.
            if (System.IO.File.Exists(filePath)) // Checks if the physical file exists.
            {
                return new FileInfo(filePath).Length; // Returns the size of the physical file in bytes.
            }
            return Content.Length; // If physical file doesn't exist (e.g., just created in memory), return content length.
        }

        // Edits the content of the file.
        public void Edit(string newContent)
        {
            Content = newContent; // Sets the Content property, which handles updating the physical file and LastModifiedDate.
            Console.WriteLine($"File '{Name}' content updated."); // Confirmation message.
        }

        // Appends content to the existing file content.
        public void AppendContent(string contentToAppend)
        {
            Content += contentToAppend; // Appends content to the existing Content.
            Console.WriteLine($"Content appended to '{Name}'."); // Confirmation message.
        }

        // Prepends content to the existing file content.
        public void PrependContent(string contentToPrepend)
        {
            Content = contentToPrepend + Content; // Prepends content to the existing Content.
            Console.WriteLine($"Content prepended to '{Name}'."); // Confirmation message.
        }

        // Inserts content at a specific line number.
        public void InsertContent(int lineNumber, string contentToInsert)
        {
            // Splits the content into individual lines.
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            // Checks if the line number is within a valid range.
            if (lineNumber < 1 || lineNumber > lines.Count + 1)
            {
                throw new ArgumentOutOfRangeException("Line number out of range."); // Throws an exception if out of range.
            }
            lines.Insert(lineNumber - 1, contentToInsert); // Inserts the new content at the specified line number (0-indexed).
            Content = string.Join(Environment.NewLine, lines); // Joins the lines back into a single string and updates Content.
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'."); // Confirmation message.
        }

        // Deletes a specific line from the file content.
        public void DeleteLine(int lineNumber)
        {
            // Splits the content into individual lines.
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            // Checks if the line number is within a valid range.
            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                throw new ArgumentOutOfRangeException("Line number out of range."); // Throws an exception if out of range.
            }
            lines.RemoveAt(lineNumber - 1); // Removes the line at the specified line number (0-indexed).
            Content = string.Join(Environment.NewLine, lines); // Joins the remaining lines back into a single string and updates Content.
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'."); // Confirmation message.
        }

        // Displays the content of the file to the console.
        public void View()
        {
            LastAccessedDate = DateTime.Now; // Updates the last accessed date.
            Console.WriteLine($"--- Content of {Name} ---"); // Header.
            Console.WriteLine(Content); // Prints the file content.
            Console.WriteLine("--------------------------"); // Footer.
        }
    }
}