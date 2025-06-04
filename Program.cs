using System; // Provides fundamental classes and base types
using System.Linq; // Provides classes and methods for LINQ
using testonly.NewFolder; // Imports the namespace containing FileSystemEntity, Folder, File, HistoryLogger, and TextEditor

namespace FileStorageSystem // Declares the namespace for the file storage system application
{
    public enum Commands // Defines an enumeration for different commands supported by the system
    {
        Help, // Displays help message
        PrintWorkingDirectory, // Prints the current working directory
        CreateFolder, // Creates a new folder
        CreateFile, // Creates a new file
        Remove, // Deletes a file or folder
        Rename, // Renames a file or folder
        Move, // Moves a file or folder
        Copy, // Copies a file or folder
        Navigate, // Changes the current directory
        ListContent, // Lists contents of a folder
        ReadFiles, // Reads content of a file
        FileEditor, // Edits content of a file
        SearchFiles, // Searches for files
        ShowHistory, // Shows file access history
        Exit, // Exits the application
        Unknown // Represents an unknown command
    }

    public class Program // Main class for the application
    {
        private static Commands GetCommands(string command) // Converts a string command into its corresponding Commands enum value
        {
            return command.ToLower() switch // Uses a switch expression for concise mapping
            {
                "help" => Commands.Help, // Maps "help" to Commands.Help
                "pwd" => Commands.PrintWorkingDirectory, // Maps "pwd" to Commands.PrintWorkingDirectory
                "mkdir" => Commands.CreateFolder, // Maps "mkdir" to Commands.CreateFolder
                "touch" => Commands.CreateFile, // Maps "touch" to Commands.CreateFile
                "rm" => Commands.Remove, // Maps "rm" to Commands.Remove
                "rename" => Commands.Rename, // Maps "rename" to Commands.Rename
                "mv" => Commands.Move, // Maps "mv" to Commands.Move
                "cp" => Commands.Copy, // Maps "cp" to Commands.Copy
                "cd" => Commands.Navigate, // Maps "cd" to Commands.Navigate
                "ls" => Commands.ListContent, // Maps "ls" to Commands.ListContent
                "cat" => Commands.ReadFiles, // Maps "cat" to Commands.ReadFiles
                "nano" => Commands.FileEditor, // Maps "nano" to Commands.FileEditor
                "search" => Commands.SearchFiles, // Maps "search" to Commands.SearchFiles
                "history" => Commands.ShowHistory, // Maps "history" to Commands.ShowHistory
                "exit" => Commands.Exit, // Maps "exit" to Commands.Exit
                _ => Commands.Unknown // Default case for unknown commands
            };
        }

        public static void Main(string[] args) // Entry point of the application
        {
            FileManager fileManager = FileManager.Load(); // Loads an existing file system or initializes a new one
            Console.WriteLine("C# File Storage System\nAuthor: Peh Yan Bin"); // Prints welcome message and author
            fileManager.PrintWorkingDirectory(); // Prints the initial working directory

            while (true) // Main application loop
            {
                Console.Write($"\n{fileManager.CurrentFolder.GetFullPath()}> "); // Displays the current directory as a prompt
                string commandLine = Console.ReadLine()?.Trim(); // Reads user input and trims whitespace

                if (string.IsNullOrEmpty(commandLine)) // If input is empty, continue to the next iteration
                {
                    continue;
                }

                string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Splits the command line into parts
                Commands command = GetCommands(parts[0].ToLower()); // Gets the command enum from the first part of the input

                try
                {
                    switch (command) // Executes actions based on the parsed command
                    {
                        case Commands.Help: // If command is Help
                            fileManager.DisplayHelp(); // Displays help message
                            break;
                        case Commands.PrintWorkingDirectory: // If command is PrintWorkingDirectory
                            fileManager.PrintWorkingDirectory(); // Prints current working directory
                            break;
                        case Commands.CreateFolder: // If command is CreateFolder
                            if (parts.Length > 1) // Checks if a folder name is provided
                            {
                                fileManager.CreateFolder(parts[1]); // Creates the folder
                            }
                            else
                            {
                                Console.WriteLine("Usage: mkdir <folder_name>"); // Prints usage if no folder name
                            }
                            break;
                        case Commands.CreateFile: // If command is CreateFile
                            if (parts.Length > 1) // Checks if a file name is provided
                            {
                                // Extracts content if provided, otherwise an empty string
                                string content = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";
                                fileManager.CreateFile(parts[1], content); // Creates the file with optional content
                            }
                            else
                            {
                                Console.WriteLine("Usage: touch <file_name> [content]"); // Prints usage if no file name
                            }
                            break;
                        case Commands.Remove: // If command is Remove
                            if (parts.Length > 1) // Checks if a path is provided
                            {
                                fileManager.DeleteEntity(parts[1]); // Deletes the entity at the specified path
                            }
                            else
                            {
                                Console.WriteLine("Usage: rm <path>"); // Prints usage if no path
                            }
                            break;
                        case Commands.Rename: // If command is Rename
                            if (parts.Length == 3) // Checks if old path and new name are provided
                            {
                                fileManager.RenameEntity(parts[1], parts[2]); // Renames the entity
                            }
                            else
                            {
                                Console.WriteLine("Usage: rename <path> <new_name>"); // Prints usage
                            }
                            break;
                        case Commands.Move: // If command is Move
                            if (parts.Length == 3) // Checks if source and destination paths are provided
                            {
                                fileManager.MoveEntity(parts[1], parts[2]); // Moves the entity
                            }
                            else
                            {
                                Console.WriteLine("Usage: mv <source_path> <destination_path>"); // Prints usage
                            }
                            break;
                        case Commands.Copy: // If command is Copy
                            if (parts.Length == 3) // Checks if source and destination paths are provided
                            {
                                fileManager.CopyEntity(parts[1], parts[2]); // Copies the entity
                            }
                            else
                            {
                                Console.WriteLine("Usage: cp <source_path> <destination_path>"); // Prints usage
                            }
                            break;
                        case Commands.Navigate: // If command is Navigate
                            if (parts.Length > 1) // Checks if a path is provided
                            {
                                fileManager.NavigateTo(parts[1]); // Navigates to the specified path
                            }
                            else
                            {
                                Console.WriteLine("Usage: cd <path>"); // Prints usage
                            }
                            break;
                        case Commands.ListContent: // If command is ListContent
                            bool detailed = parts.Contains("-l"); // Checks if the "-l" flag for detailed view is present
                            string lsPath = null; // Initializes path for listing
                            if (parts.Length > 1 && !parts[1].Equals("-l", StringComparison.OrdinalIgnoreCase)) // If path is provided and not "-l"
                            {
                                lsPath = parts[1]; // Set lsPath to the provided path
                            }
                            else if (parts.Length > 2 && parts[1].Equals("-l", StringComparison.OrdinalIgnoreCase)) // If "-l" is present and a path is also provided
                            {
                                lsPath = parts[2]; // Set lsPath to the path after "-l"
                            }
                            fileManager.ListContents(lsPath, detailed); // Lists contents
                            break;
                        case Commands.ReadFiles: // If command is ReadFiles
                            if (parts.Length > 1) // Checks if a file path is provided
                            {
                                fileManager.ViewFile(parts[1]); // Views the content of the file
                            }
                            else
                            {
                                Console.WriteLine("Usage: cat <file_path>"); // Prints usage
                            }
                            break;
                        case Commands.FileEditor: // If command is FileEditor
                            if (parts.Length > 1) // Checks if a file path is provided
                            {
                                fileManager.EditFile(parts[1]); // Edits the file
                            }
                            else
                            {
                                Console.WriteLine("Usage: nano <file_path>"); // Prints usage
                            }
                            break;
                        case Commands.SearchFiles: // If command is SearchFiles
                            if (parts.Length > 1) // Checks if a search term is provided
                            {
                                string searchTerm = parts[1]; // Gets the search term
                                fileManager.SearchFiles(searchTerm); // Searches for files
                            }
                            else
                            {
                                Console.WriteLine("Usage: search <term>"); // Prints usage
                            }
                            break;
                        case Commands.ShowHistory: // If command is ShowHistory
                            fileManager.DisplayHistory(); // Displays file access history
                            break;
                        case Commands.Exit: // If command is Exit
                            fileManager.Save(); // Saves the file system state
                            Console.WriteLine("File System shutted down."); // Prints exit message
                            return; // Exits the application
                        case Commands.Unknown: // If command is Unknown
                            Console.WriteLine($"Error: Unknown command '{parts[0]}'. Type 'help' for available commands."); // Informs user about unknown command
                            break;
                    }
                }
                catch (Exception ex) // Catches any exceptions thrown during command execution
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Prints the error message
                }
            }
        }
    }
}