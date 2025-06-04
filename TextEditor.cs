using System; // Provides fundamental classes and base types
using System.Collections.Generic; // Provides interfaces and classes that define generic collections
using System.Linq; // Provides classes and methods for LINQ

namespace testonly.NewFolder // Declares a namespace for organizing related code
{
    public static class TextEditor // Defines a static class for text editing functionalities
    {
        public static string EditText(string initialContent) // Method to interactively edit text
        {
            Console.WriteLine("\n--- Text Editor (Type 'SAVE' on a new line to save file and exit, 'QUIT' to exit without saving, 'DELETE' for deleting lines and 'INSERT' for inserting lines) ---"); // Instructions for the user
            // Splits initial content into a list of lines
            List<string> lines = new List<string>(initialContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

            Console.WriteLine("Initial Content:"); // Header for initial content display
            DisplayContentWithLineNumbers(lines); // Displays the initial content with line numbers

            while (true) // Loop for continuous editing
            {
                Console.Write($"{lines.Count + 1}> "); // Displays the next line number for input
                string input = Console.ReadLine(); // Reads user input

                if (input.Equals("SAVE", StringComparison.OrdinalIgnoreCase)) // If input is "SAVE" (case-insensitive)
                {
                    Console.WriteLine("Changes saved"); // Confirmation message
                    return string.Join(Environment.NewLine, lines); // Joins lines back into a single string and returns
                }
                else if (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase)) // If input is "QUIT" (case-insensitive)
                {
                    Console.WriteLine("Discard changes"); // Confirmation message
                    return initialContent; // Returns the original content, discarding changes
                }
                else if (input.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase)) // If input starts with "INSERT "
                {
                    string[] parts = input.Split(new char[] { ' ' }, 3); // Splits the input into parts (command, line number, content)
                    // Tries to parse the line number
                    if (parts.Length == 3 && int.TryParse(parts[1], out int lineNumber))
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count + 1) // Checks if line number is valid
                        {
                            lines.Insert(lineNumber - 1, parts[2]); // Inserts the content at the specified line number
                            Console.WriteLine($"Line inserted at {lineNumber}."); // Confirmation message
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for insert."); // Error for invalid line number
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid INSERT command. Usage: INSERT <line_number> <content>"); // Error for invalid INSERT command format
                    }
                }
                else if (input.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase)) // If input starts with "DELETE "
                {
                    string[] parts = input.Split(' '); // Splits the input into parts (command, line number)
                    // Tries to parse the line number
                    if (parts.Length == 2 && int.TryParse(parts[1], out int lineNumber))
                    {
                        if (lineNumber >= 1 && lineNumber <= lines.Count) // Checks if line number is valid
                        {
                            lines.RemoveAt(lineNumber - 1); // Removes the line at the specified line number
                            Console.WriteLine($"Line {lineNumber} deleted."); // Confirmation message
                        }
                        else
                        {
                            Console.WriteLine("Invalid line number for delete."); // Error for invalid line number
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid DELETE command. Usage: DELETE <line_number>"); // Error for invalid DELETE command format
                    }
                }
                else // If input is not a command, it's treated as new content to append
                {
                    lines.Add(input); // Adds the input as a new line
                }
                DisplayContentWithLineNumbers(lines); // Displays the updated content with line numbers
            }
        }

        public static void ViewText(string content) // Method to display text content with line numbers
        {
            Console.WriteLine("\n--- Viewing File Content ---"); // Header for viewing content
            // Splits content into lines and displays them with line numbers
            DisplayContentWithLineNumbers(content.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList());
            Console.WriteLine("----------------------------"); // Footer for viewing content
        }

        public static void DisplayContentWithLineNumbers(List<string> lines) // Helper method to display lines with line numbers
        {
            for (int i = 0; i < lines.Count; i++) // Loops through each line
            {
                Console.WriteLine($"{i + 1}: {lines[i]}"); // Prints the line number and the line content
            }
        }
    }
}