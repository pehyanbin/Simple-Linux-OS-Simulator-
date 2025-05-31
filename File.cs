using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using testonly.NewFolder;

namespace FileStorageSystem
{
    public class File : FileSystemEntity
    {
        public string FilePath { get; private set; }

        [JsonConstructor]
        public File(string name, DateTime creationDate, DateTime lastModifiedDate, DateTime lastAccessedDate)
            : base(name, null)
        {
            CreationDate = creationDate;
            LastModifiedDate = lastModifiedDate;
            LastAccessedDate = lastAccessedDate;
        }

        public File(string name, Folder parent, string content = "") : base(name, parent)
        {
            FilePath = Path.Combine(GetPhysicalFolderPath(), $"{name}.txt");
            System.IO.File.WriteAllText(FilePath, content);
        }

        private string GetPhysicalFolderPath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = ParentFolder?.GetFullPath() ?? "root";
            return Path.Combine(basePath, folderPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        public string GetContent()
        {
            LastAccessedDate = DateTime.Now;
            return System.IO.File.Exists(FilePath) ? System.IO.File.ReadAllText(FilePath) : "";
        }

        public void SetContent(string content)
        {
            System.IO.File.WriteAllText(FilePath, content);
            LastModifiedDate = DateTime.Now;
        }

        public override long GetSize()
        {
            if (System.IO.File.Exists(FilePath))
            {
                return new FileInfo(FilePath).Length;
            }
            return 0;
        }

        public void Edit(string newContent)
        {
            SetContent(newContent);
            Console.WriteLine($"File '{Name}' content updated.");
        }

        public void AppendContent(string contentToAppend)
        {
            System.IO.File.AppendAllText(FilePath, contentToAppend);
            LastModifiedDate = DateTime.Now;
            Console.WriteLine($"Content appended to '{Name}'.");
        }

        public void PrependContent(string contentToPrepend)
        {
            string currentContent = GetContent();
            SetContent(contentToPrepend + currentContent);
            Console.WriteLine($"Content prepended to '{Name}'.");
        }

        public void InsertContent(int lineNumber, string contentToInsert)
        {
            var lines = GetContent().Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count + 1)
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.Insert(lineNumber - 1, contentToInsert);
            SetContent(string.Join(Environment.NewLine, lines));
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'.");
        }

        public void DeleteLine(int lineNumber)
        {
            var lines = GetContent().Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.RemoveAt(lineNumber - 1);
            SetContent(string.Join(Environment.NewLine, lines));
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'.");
        }

        public void View()
        {
            LastAccessedDate = DateTime.Now;
            Console.WriteLine($"--- Content of {Name} ---");
            Console.WriteLine(GetContent());
            Console.WriteLine("--------------------------");
        }

        public void UpdateFilePath()
        {
            string oldPath = FilePath;
            FilePath = Path.Combine(GetPhysicalFolderPath(), $"{Name}.txt");
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Move(oldPath, FilePath);
            }
        }

        public void DeletePhysicalFile()
        {
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }
        }
    }
}