using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorageSystem
{
    public static class TextEditor
    {
        public static string EditText(string initialContent)
        {
            Console.WriteLine("\n--- Text Editor (Type 'SAVE' on a new line to save file and exit, 'QUIT' to exit without saving) ---");
            List<string> lines = new List<string>(initialContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

            Console.WriteLine("Initial Content:");
            DisplayContentWithLineNumbers(lines);

            while (true)
            {
                Console.Write($"{lines.Count + 1}> ");
                string input = Console.ReadLine();

                if (input.Equals("SAVE", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Changes saved");
                    return string.Join(Environment.NewLine, lines);
                }
                else if (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Discard changes");
                    return initialContent;
                }
                else if (input.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = input.Split(new char[] { ' ' }, 3);
                    if (parts.Length == 3 && int.TryParse(parts[1], out int lineNumber))
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count + 1)
                        {
                            lines.Insert(lineNumber - 1, parts[2]);
                            Console.WriteLine($"Line inserted at {lineNumber}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for insert.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid INSERT command. Usage: INSERT <line_number> <content>");
                    }
                }
                else if (input.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = input.Split(' ');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lineNumber))
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count)
                        {
                            lines.RemoveAt(lineNumber - 1);
                            Console.WriteLine($"Line {lineNumber} deleted.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for delete.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid DELETE command. Usage: DELETE <line_number>");
                    }
                }
                else
                {
                    lines.Add(input);
                }
                DisplayContentWithLineNumbers(lines);
            }
        }

        public static void ViewText(string content)
        {
            Console.WriteLine("\n--- Viewing File Content ---");
            DisplayContentWithLineNumbers(content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList());
            Console.WriteLine("----------------------------");
        }

        public static void DisplayContentWithLineNumbers(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {lines[i]}");
            }
        }
    }
}