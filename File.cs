using System;
using System.IO;
using System.Linq;

namespace FileStorageSystem
{
    public class File : FileSystemEntity
    {
        private string _content;
        public string Content
        {
            get
            {
                LastAccessedDate = DateTime.Now;
                string filePath = GetFullPath();
                if (System.IO.File.Exists(filePath))
                {
                    _content = System.IO.File.ReadAllText(filePath);
                }
                return _content;
            }
            set
            {
                _content = value;
                LastModifiedDate = DateTime.Now;
                string filePath = GetFullPath();
                System.IO.File.WriteAllText(filePath, _content);
            }
        }

        public File(string name, Folder parent, string content = "") : base(name, parent)
        {
            _content = content;
            string filePath = GetFullPath();
            
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath+".txt", content);
            }
        }

        public override long GetSize()
        {
            string filePath = GetFullPath();
            if (System.IO.File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }
            return Content.Length;
        }

        public void Edit(string newContent)
        {
            Content = newContent;
            Console.WriteLine($"File '{Name}' content updated.");
        }

        public void AppendContent(string contentToAppend)
        {
            Content += contentToAppend;
            Console.WriteLine($"Content appended to '{Name}'.");
        }

        public void PrependContent(string contentToPrepend)
        {
            Content = contentToPrepend + Content;
            Console.WriteLine($"Content prepended to '{Name}'.");
        }

        public void InsertContent(int lineNumber, string contentToInsert)
        {
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count + 1)
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.Insert(lineNumber - 1, contentToInsert);
            Content = string.Join(Environment.NewLine, lines);
            Console.WriteLine($"Content inserted at line {lineNumber} in '{Name}'.");
        }

        public void DeleteLine(int lineNumber)
        {
            var lines = Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            if (lineNumber < 1 || lineNumber > lines.Count)
            {
                throw new ArgumentOutOfRangeException("Line number out of range.");
            }
            lines.RemoveAt(lineNumber - 1);
            Content = string.Join(Environment.NewLine, lines);
            Console.WriteLine($"Line {lineNumber} deleted from '{Name}'.");
        }

        public void View()
        {
            LastAccessedDate = DateTime.Now;
            Console.WriteLine($"--- Content of {Name} ---");
            Console.WriteLine(Content);
            Console.WriteLine("--------------------------");
        }
    }
}