using System;
using System.IO;
using System.Text.Json.Serialization;

namespace testonly.NewFolder
{
    public abstract class FileSystemEntity
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("lastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonPropertyName("lastAccessedDate")]
        public DateTime LastAccessedDate { get; set; }

        [JsonIgnore]
        public Folder ParentFolder { get; set; }

        protected FileSystemEntity(string name, Folder parent)
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
            if (ParentFolder == null)
            {
                return Name;
            }
            return Path.Combine(ParentFolder.GetFullPath(), Name);
        }
    }
}