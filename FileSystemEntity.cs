using System; // Provides fundamental classes and base types
using System.IO; // Provides types for reading and writing files and data streams, and types for basic file and directory support
using System.Text.Json.Serialization; // Provides attributes for controlling how JSON serialization and deserialization is performed

namespace testonly.NewFolder // Declares a namespace for organizing related code
{
    public abstract class FileSystemEntity // Defines an abstract base class for all file system entities (files and folders)
    {
        [JsonPropertyName("name")] // Maps the 'Name' property to a JSON property named "name"
        public string Name { get; set; } // Gets or sets the name of the file system entity

        [JsonPropertyName("creationDate")] // Maps the 'CreationDate' property to a JSON property named "creationDate"
        public DateTime CreationDate { get; set; } // Gets or sets the creation date and time of the entity

        [JsonPropertyName("lastModifiedDate")] // Maps the 'LastModifiedDate' property to a JSON property named "lastModifiedDate"
        public DateTime LastModifiedDate { get; set; } // Gets or sets the last modification date and time of the entity

        [JsonPropertyName("lastAccessedDate")] // Maps the 'LastAccessedDate' property to a JSON property named "lastAccessedDate"
        public DateTime LastAccessedDate { get; set; } // Gets or sets the last access date and time of the entity

        [JsonIgnore] // Instructs the JSON serializer to ignore this property
        public Folder ParentFolder { get; set; } // Gets or sets the parent folder of the entity (not serialized to avoid circular references)

        protected FileSystemEntity(string name, Folder parent) // Constructor for FileSystemEntity
        {
            Name = name; // Initializes the Name property
            CreationDate = DateTime.Now; // Sets CreationDate to the current date and time
            LastModifiedDate = DateTime.Now; // Sets LastModifiedDate to the current date and time
            LastAccessedDate = DateTime.Now; // Sets LastAccessedDate to the current date and time
            ParentFolder = parent; // Initializes the ParentFolder property
        }

        public void Rename(string newName) // Renames the file system entity
        {
            if (string.IsNullOrWhiteSpace(newName)) // Checks if the new name is null, empty, or consists only of white-space characters
            {
                throw new ArgumentException("New name cannot be empty."); // Throws an exception if the new name is invalid
            }
            // Checks if an entity with the new name (case-insensitive) already exists in the parent folder
            if (ParentFolder != null && ParentFolder.Contents.Exists(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{newName}' already exists in this folder."); // Throws an exception if an entity with the new name already exists
            }
            Name = newName; // Sets the new name
            LastModifiedDate = DateTime.Now; // Updates LastModifiedDate
            Console.WriteLine($"Renamed to: {newName}"); // Prints a confirmation message
        }

        public abstract long GetSize(); // Abstract method to get the size of the entity; must be implemented by derived classes

        public string GetFullPath() // Gets the full path of the file system entity
        {
            if (ParentFolder == null) // If there is no parent folder (i.e., it's the root)
            {
                return Name; // Return the entity's name as its full path
            }
            // Otherwise, combine the parent folder's full path with the entity's name
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}