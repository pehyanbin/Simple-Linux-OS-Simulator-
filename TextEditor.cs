using System; // Imports the System namespace, providing fundamental classes and base types.
using System.Collections.Generic; // Imports the System.Collections.Generic namespace, for generic collection types like List.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public static class TextEditor // Declares a public static class TextEditor, meaning it contains only static members and cannot be instantiated.
    {
        // Provides an interactive text editing experience.
        public static string EditText(string initialContent)
        {
            Console.WriteLine("\n--- Text Editor (Type 'SAVE' on a new line to save file and exit, 'QUIT' to exit without saving) ---"); // Instructions for the user.
            // Splits the initial content into a list of lines, handling different newline characters.
            List<string> lines = new List<string>(initialContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

            Console.WriteLine("Initial Content:"); // Header for initial content display.
            DisplayContentWithLineNumbers(lines); // Displays the initial content with line numbers.

            while (true) // Enters an infinite loop for user input within the editor.
            {
                Console.Write($"{lines.Count + 1}> "); // Displays the next line number as a prompt.
                string input = Console.ReadLine(); // Reads a line of input from the console.

                if (input.Equals("SAVE", StringComparison.OrdinalIgnoreCase)) // If the input is "SAVE" (case-insensitive).
                {
                    Console.WriteLine("Changes saved"); // Confirmation message.
                    return string.Join(Environment.NewLine, lines); // Joins the lines back into a single string with newline characters and returns it.
                }
                else if (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase)) // If the input is "QUIT" (case-insensitive).
                {
                    Console.WriteLine("Discard changes"); // Confirmation message.
                    return initialContent; // Returns the initial content, discarding any changes.
                }
                else if (input.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase)) // If the input starts with "INSERT ".
                {
                    // Splits the input into parts: "INSERT", line number, and content. Max 3 parts.
                    string[] parts = input.Split(new char[] { ' ' }, 3);
                    // Checks if there are enough parts and if the line number is a valid integer.
                    if (parts.Length == 3 && int.TryParse(parts[1], out int lineNumber))
                    {
                        // Checks if the line number is within the valid range (1 to total lines + 1 for appending).
                        if (lineNumber >= 1 && lineNumber <= lines.Count + 1)
                        {
                            lines.Insert(lineNumber - 1, parts[2]); // Inserts the content at the specified 0-indexed line number.
                            Console.WriteLine($"Line inserted at {lineNumber}."); // Confirmation message.
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for insert."); // Error for invalid line number.
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid INSERT command. Usage: INSERT <line_number> <content>"); // Usage instructions for INSERT.
                    }
                }
                else if (input.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase)) // If the input starts with "DELETE ".
                {
                    string[] parts = input.Split(' '); // Splits the input into "DELETE" and line number.
                    // Checks if there are enough parts and if the line number is a valid integer.
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lineNumber))
                    {
                        // Checks if the line number is within the valid range (1 to total lines).
                        if (lineNumber >= 1 && lineNumber <= lines.Count)
                        {
                            lines.RemoveAt(lineNumber - 1); // Removes the line at the specified 0-indexed line number.
                            Console.WriteLine($"Line {lineNumber} deleted."); // Confirmation message.
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for delete."); // Error for invalid line number.
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid DELETE command. Usage: DELETE <line_number>"); // Usage instructions for DELETE.
                    }
                }
                else // If the input is neither a special command nor an editor command.
                {
                    lines.Add(input); // Adds the input as a new line to the end of the content.
                }
                DisplayContentWithLineNumbers(lines); // Redisplays the updated content with line numbers.
            }
        }

        // Displays file content for viewing purposes.
        public static void ViewText(string content)
        {
            Console.WriteLine("\n--- Viewing File Content ---"); // Header for viewing.
            // Splits the content into lines and displays them with line numbers.
            DisplayContentWithLineNumbers(content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList());
            Console.WriteLine("----------------------------"); // Footer.
        }

        // Helper method to display a list of strings with line numbers.
        public static void DisplayContentWithLineNumbers(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++) // Loops through each line.
            {
                Console.WriteLine($"{i + 1}: {lines[i]}"); // Prints the line number (1-indexed) followed by the line content.
            }
        }
    }
}