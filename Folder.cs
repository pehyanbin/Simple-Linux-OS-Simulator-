using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileStorageSystem
{
    public class Folder : FileSystemEntity
    {
        private List<FileSystemEntity> _contents = new List<FileSystemEntity>();

        public List<FileSystemEntity> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        public Folder(string name, Folder parent) : base(name, parent)
        {
            _contents = new List<FileSystemEntity>();
            string fullPath = GetFullPath();
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                CreationDate = Directory.GetCreationTime(fullPath);
                LastModifiedDate = Directory.GetLastWriteTime(fullPath);
                LastAccessedDate = Directory.GetLastAccessTime(fullPath);
            }
        }

        public void AddEntity(FileSystemEntity entity)
        {
            if (Contents.Any(e => e.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists in this folder.");
            }
            Contents.Add(entity);
            entity.ParentFolder = this;
            LastModifiedDate = DateTime.Now;
        }

        public void RemoveEntity(FileSystemEntity entity)
        {
            if (Contents.Remove(entity))
            {
                string entityPath = Path.Combine(GetFullPath(), entity.Name);
                if (entity is Folder)
                {
                    if (Directory.Exists(entityPath))
                    {
                        Directory.Delete(entityPath, true);
                    }
                }
                else if (entity is File)
                {
                    if (System.IO.File.Exists(entityPath))
                    {
                        System.IO.File.Delete(entityPath);
                    }
                }
                LastModifiedDate = DateTime.Now;
            }
            else
            {
                throw new InvalidOperationException($"Entity '{entity.Name}' not found in folder '{Name}'.");
            }
        }

        public FileSystemEntity GetEntity(string name)
        {
            return Contents.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override long GetSize()
        {
            long totalSize = 0;
            foreach (var entity in Contents)
            {
                totalSize += entity.GetSize();
            }
            return totalSize;
        }

        public void ListContents(bool detailed = false)
        {
            LastAccessedDate = DateTime.Now;
            Console.WriteLine($"Contents of {GetFullPath()}:");
            if (!Contents.Any())
            {
                Console.WriteLine("  (Empty)");
                return;
            }

            foreach (var entity in Contents.OrderBy(e => e.Name))
            {
                if (detailed)
                {
                    string type = entity is Folder ? "DIR" : "FIL";
                    Console.WriteLine($"  {type} {entity.Name,-20} Size: {entity.GetSize(),-8} Created: {entity.CreationDate.ToShortDateString()} {entity.CreationDate.ToShortTimeString()} Modified: {entity.LastModifiedDate.ToShortDateString()} {entity.LastModifiedDate.ToShortTimeString()} Accessed: {entity.LastAccessedDate.ToShortDateString()} {entity.LastAccessedDate.ToShortTimeString()}");
                }
                else
                {
                    Console.WriteLine($"  {entity.Name}{(entity is Folder ? "/" : "")}");
                }
            }
        }
    }
}