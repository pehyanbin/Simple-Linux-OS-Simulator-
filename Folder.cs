using System; // Imports the System namespace, providing fundamental classes and base types.
using System.Collections.Generic; // Imports the System.Collections.Generic namespace, for generic collection types like List.
using System.IO; // Imports the System.IO namespace, for input/output operations like file and directory manipulation.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public class Folder : FileSystemEntity // Declares the public class Folder, inheriting from FileSystemEntity.
    {
        private List<FileSystemEntity> _contents = new List<FileSystemEntity>(); // Private field to store the list of entities (files/folders) within this folder.

        public List<FileSystemEntity> Contents // Public property to get or set the list of contents.
        {
            get { return _contents; } // Getter for Contents.
            set { _contents = value; } // Setter for Contents.
        }

        public Folder(string name, Folder parent) : base(name, parent) // Constructor for the Folder class, calling the base constructor.
        {
            _contents = new List<FileSystemEntity>(); // Initializes the contents list.
            string fullPath = GetFullPath(); // Gets the full physical path for this folder.
            if (!Directory.Exists(fullPath)) // Checks if the physical directory does not exist.
            {
                Directory.CreateDirectory(fullPath); // Creates the physical directory.
                CreationDate = Directory.GetCreationTime(fullPath); // Sets the creation date from the physical directory.
                LastModifiedDate = Directory.GetLastWriteTime(fullPath); // Sets the last modified date from the physical directory.
                LastAccessedDate = Directory.GetLastAccessTime(fullPath); // Sets the last accessed date from the physical directory.
            }
        }

        // Adds a FileSystemEntity (file or folder) to this folder's contents.
        public void AddEntity(FileSystemEntity entity)
        {
            // Checks if an entity with the same name (case-insensitive) already exists.
            if (Contents.Any(e => e.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists in this folder."); // Throws an exception if a duplicate name is found.
            }
            Contents.Add(entity); // Adds the entity to the internal list.
            entity.ParentFolder = this; // Sets the parent folder of the added entity to this folder.
            LastModifiedDate = DateTime.Now; // Updates the last modified date of this folder.
        }

        // Removes a FileSystemEntity from this folder's contents and its physical counterpart.
        public void RemoveEntity(FileSystemEntity entity)
        {
            if (Contents.Remove(entity)) // Tries to remove the entity from the internal list.
            {
                string entityPath = Path.Combine(GetFullPath(), entity.Name); // Constructs the full physical path of the entity to be removed.
                if (entity is Folder) // If the entity is a folder.
                {
                    if (Directory.Exists(entityPath)) // Checks if the physical directory exists.
                    {
                        Directory.Delete(entityPath, true); // Deletes the physical directory and its contents recursively.
                    }
                }
                else if (entity is File) // If the entity is a file.
                {
                    if (System.IO.File.Exists(entityPath)) // Checks if the physical file exists.
                    {
                        System.IO.File.Delete(entityPath); // Deletes the physical file.
                    }
                }
                LastModifiedDate = DateTime.Now; // Updates the last modified date of this folder.
            }
            else
            {
                throw new InvalidOperationException($"Entity '{entity.Name}' not found in folder '{Name}'."); // Throws an exception if the entity was not found in the contents.
            }
        }

        // Retrieves a FileSystemEntity by its name from this folder's contents.
        public FileSystemEntity GetEntity(string name)
        {
            // Uses LINQ to find the first entity whose name matches (case-insensitive).
            return Contents.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override long GetSize() // Overrides the abstract GetSize method from FileSystemEntity.
        {
            long totalSize = 0; // Initializes total size to 0.
            foreach (var entity in Contents) // Iterates through each entity in this folder's contents.
            {
                totalSize += entity.GetSize(); // Adds the size of each entity to the total size.
            }
            return totalSize; // Returns the accumulated total size.
        }

        // Lists the contents of the folder, with an option for detailed view.
        public void ListContents(bool detailed = false)
        {
            LastAccessedDate = DateTime.Now; // Updates the last accessed date of this folder.
            Console.WriteLine($"Contents of {GetFullPath()}:"); // Prints the full path of the folder.
            if (!Contents.Any()) // Checks if the folder is empty.
            {
                Console.WriteLine("  (Empty)"); // Prints "(Empty)" if no contents.
                return;
            }

            foreach (var entity in Contents.OrderBy(e => e.Name)) // Iterates through contents, ordered by name.
            {
                if (detailed) // If detailed view is requested.
                {
                    string type = entity is Folder ? "DIR" : "FIL"; // Determines if the entity is a directory or file.
                    // Prints detailed information: type, name, size, creation date, modification date, access date.
                    Console.WriteLine($"  {type} {entity.Name,-20} Size: {entity.GetSize(),-8} Created: {entity.CreationDate.ToShortDateString()} {entity.CreationDate.ToShortTimeString()} Modified: {entity.LastModifiedDate.ToShortDateString()} {entity.LastModifiedDate.ToShortTimeString()} Accessed: {entity.LastAccessedDate.ToShortDateString()} {entity.LastAccessedDate.ToShortTimeString()}");
                }
                else // If simple view.
                {
                    // Prints just the name, appending '/' for folders.
                    Console.WriteLine($"  {entity.Name}{(entity is Folder ? "/" : "")}");
                }
            }
        }
    }
}