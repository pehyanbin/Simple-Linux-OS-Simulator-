using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace testonly.NewFolder
{
    public class Folder : FileSystemEntity
    {
        [JsonPropertyName("contents")]
        public List<FileSystemEntity> Contents { get; set; } = new List<FileSystemEntity>();

        [JsonConstructor]
        public Folder(string name, List<FileSystemEntity> contents, DateTime creationDate, DateTime lastModifiedDate, DateTime lastAccessedDate)
            : base(name, null)
        {
            Contents = contents ?? new List<FileSystemEntity>();
            CreationDate = creationDate;
            LastModifiedDate = lastModifiedDate;
            LastAccessedDate = lastAccessedDate;
        }

        public Folder(string name, Folder parent) : base(name, parent)
        {
            Contents = new List<FileSystemEntity>();
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