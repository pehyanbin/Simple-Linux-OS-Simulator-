using System; // Provides fundamental classes and base types
using System.Collections.Generic; // Provides interfaces and classes that define generic collections
using System.IO.Enumeration; // Not directly used in the provided code, but typically for enumerating file system entries
using System.Linq; // Provides classes and methods for LINQ (Language Integrated Query)
using System.Text.Json.Serialization; // Provides attributes for controlling how JSON serialization and deserialization is performed
using System.Xml.Linq; // Not directly used in the provided code, but typically for XML manipulation

namespace testonly.NewFolder // Declares a namespace for organizing related code
{
    public class Folder : FileSystemEntity // Defines the Folder class, inheriting from FileSystemEntity
    {
        [JsonPropertyName("contents")] // Maps the 'Contents' property to a JSON property named "contents"
        public List<FileSystemEntity> Contents { get; set; } = new List<FileSystemEntity>(); // Gets or sets the list of file system entities contained within this folder

        [JsonConstructor] // Specifies a constructor to be used by the JSON deserializer
        public Folder(string name, List<FileSystemEntity> contents, DateTime creationDate, DateTime lastModifiedDate, DateTime lastAccessedDate)
            : base(name, null) // Calls the base class constructor with the name and a null parent
        {
            Contents = contents ?? new List<FileSystemEntity>(); // Initializes Contents, using an empty list if the provided contents are null
            CreationDate = creationDate; // Sets the CreationDate
            LastModifiedDate = lastModifiedDate; // Sets the LastModifiedDate
            LastAccessedDate = lastAccessedDate; // Sets the LastAccessedDate
        }

        public Folder(string name, Folder parent) : base(name, parent) // Constructor for creating a new folder with a parent
        {
            Contents = new List<FileSystemEntity>(); // Initializes Contents as an empty list
        }

        public void AddEntity(FileSystemEntity entity) // Adds a file system entity to the folder
        {
            if (Contents.Any(e => e.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))) // Checks if an entity with the same name already exists (case-insensitive)
            {
                throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists in this folder."); // Throws an exception if a duplicate name is found
            }
            Contents.Add(entity); // Adds the entity to the Contents list
            entity.ParentFolder = this; // Sets the parent folder of the added entity to the current folder
            LastModifiedDate = DateTime.Now; // Updates the LastModifiedDate of the folder
        }

        public void RemoveEntity(FileSystemEntity entity) // Removes a file system entity from the folder
        {
            if (Contents.Remove(entity)) // Attempts to remove the entity from the Contents list
            {
                LastModifiedDate = DateTime.Now; // Updates LastModifiedDate if removal was successful
            }
            else
            {
                throw new InvalidOperationException($"Entity '{entity.Name}' not found in folder '{Name}'."); // Throws an exception if the entity was not found
            }
        }

        public FileSystemEntity GetEntity(string name) // Retrieves an entity by its name from the folder's contents
        {
            // Returns the first entity with a matching name (case-insensitive), or null if not found
            return Contents.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override long GetSize() // Overrides the abstract GetSize method from FileSystemEntity
        {
            long totalSize = 0; // Initializes total size
            foreach (var entity in Contents) // Iterates through all entities in the folder
            {
                totalSize += entity.GetSize(); // Adds the size of each entity to the total
            }
            return totalSize; // Returns the total size of all contents
        }

        public void ListContents(bool detailed = false) // Lists the contents of the folder, with an option for detailed view
        {
            LastAccessedDate = DateTime.Now; // Updates the LastAccessedDate of the folder
            Console.WriteLine($"Contents of {GetFullPath()}:"); // Prints the full path of the folder
            if (!Contents.Any()) // Checks if the folder is empty
            {
                Console.WriteLine("  (Empty)"); // Prints "(Empty)" if the folder has no contents
                return; // Exits the method
            }

            foreach (var entity in Contents.OrderBy(e => e.Name)) // Iterates through contents, ordered by name
            {
                if (detailed) // If detailed view is requested
                {
                    string type = entity is Folder ? "DIR" : "FIL"; // Determines if the entity is a directory or file
                    // Prints detailed information about the entity
                    Console.WriteLine($"  {type} {entity.Name,-20} Size: {entity.GetSize(),-8} Created: {entity.CreationDate.ToShortDateString()} {entity.CreationDate.ToShortTimeString()} Modified: {entity.LastModifiedDate.ToShortDateString()} {entity.LastModifiedDate.ToShortTimeString()} Accessed: {entity.LastAccessedDate.ToShortDateString()} {entity.LastAccessedDate.ToShortTimeString()}");
                }
                else // If simple view is requested
                {
                    // Prints the entity name, appending a '/' if it's a folder
                    Console.WriteLine($"  {entity.Name}{(entity is Folder ? "/" : "")}");
                }
            }
        }
    }
}