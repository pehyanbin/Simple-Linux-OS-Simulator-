using System; // Imports the System namespace, providing fundamental classes and base types.
using System.IO; // Imports the System.IO namespace, for input/output operations like file and directory manipulation.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public abstract class FileSystemEntity // Declares the abstract base class FileSystemEntity.
    {
        private string _name; // Private field to store the name of the entity.
        private DateTime _creationDate; // Private field to store the creation date.
        private DateTime _lastModifiedDate; // Private field to store the last modified date.
        private DateTime _lastAccessedDate; // Private field to store the last accessed date.
        private Folder _parentFolder; // Private field to store a reference to the parent folder.

        public string Name // Public property for the entity's name.
        {
            get { return _name; } // Getter for Name.
            set { _name = value; } // Setter for Name.
        }
        public DateTime CreationDate // Public property for the creation date.
        {
            get { return _creationDate; } // Getter for CreationDate.
            set { _creationDate = value; } // Setter for CreationDate.
        }
        public DateTime LastModifiedDate // Public property for the last modified date.
        {
            get { return _lastModifiedDate; } // Getter for LastModifiedDate.
            set { _lastModifiedDate = value; } // Setter for LastModifiedDate.
        }
        public DateTime LastAccessedDate // Public property for the last accessed date.
        {
            get { return _lastAccessedDate; } // Getter for LastAccessedDate.
            set { _lastAccessedDate = value; } // Setter for LastAccessedDate.
        }
        public Folder ParentFolder // Public property for the parent folder.
        {
            get { return _parentFolder; } // Getter for ParentFolder.
            set { _parentFolder = value; } // Setter for ParentFolder.
        }

        // Constructor for FileSystemEntity.
        public FileSystemEntity(string name, Folder parent)
        {
            Name = name; // Initializes the name.
            CreationDate = DateTime.Now; // Sets creation date to current time.
            LastModifiedDate = DateTime.Now; // Sets last modified date to current time.
            LastAccessedDate = DateTime.Now; // Sets last accessed date to current time.
            ParentFolder = parent; // Sets the parent folder.
        }

        // Renames the file system entity.
        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) // Checks if the new name is null, empty, or whitespace.
            {
                throw new ArgumentException("New name cannot be empty."); // Throws an exception if the new name is invalid.
            }
            // Checks if a duplicate name (case-insensitive) already exists in the parent folder's contents.
            // This check should only apply if the newName is different from the current name.
            if (ParentFolder != null && !Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                ParentFolder.Contents.Exists(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{newName}' already exists in this folder."); // Throws an exception if a duplicate name exists.
            }
            Name = newName; // Updates the name.
            LastModifiedDate = DateTime.Now; // Updates the last modified date.
            Console.WriteLine($"Renamed to: {newName}"); // Prints a confirmation message.
        }

        public abstract long GetSize(); // Abstract method to get the size of the entity (to be implemented by derived classes).

        // Gets the full physical path of the file system entity.
        public string GetFullPath()
        {
            // Constructs the base directory path by combining application's base directory with "FileStorage".
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");

            // If this entity has no parent (i.e., it's the root).
            if (ParentFolder == null)
            {
                return Path.Combine(basePath, Name); // Its full path is the base path combined with its name.
            }
            // Otherwise, recursively gets the parent's full path and combines it with its own name.
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}
