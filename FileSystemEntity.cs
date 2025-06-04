using System;
using System.IO;

namespace FileStorageSystem
{
    public abstract class FileSystemEntity
    {
        private string _name;
        private DateTime _creationDate;
        private DateTime _lastModifiedDate;
        private DateTime _lastAccessedDate;
        private Folder _parentFolder;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { _creationDate = value; }
        }
        public DateTime LastModifiedDate
        {
            get { return _lastModifiedDate; }
            set { _lastModifiedDate = value; }
        }
        public DateTime LastAccessedDate
        {
            get { return _lastAccessedDate; }
            set { _lastAccessedDate = value; }
        }
        public Folder ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        public FileSystemEntity(string name, Folder parent)
        {
            Name = name;
            CreationDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
            LastAccessedDate = DateTime.Now;
            ParentFolder = parent;
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New name cannot be empty.");
            }
            if (ParentFolder != null && ParentFolder.Contents.Exists(e => e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{newName}' already exists in this folder.");
            }
            Name = newName;
            LastModifiedDate = DateTime.Now;
            Console.WriteLine($"Renamed to: {newName}");
        }

        public abstract long GetSize();

        public string GetFullPath()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            if (ParentFolder == null)
            {
                return Path.Combine(basePath, Name);
            }
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}