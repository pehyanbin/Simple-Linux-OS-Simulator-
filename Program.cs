using System; // Imports the System namespace, providing fundamental classes and base types.
using System.Data; // This namespace is generally for ADO.NET and data access; it seems unused here.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    // Defines an enumeration for different commands supported by the file system.
    public enum Commands
    {
        Help, // Displays help message.
        PrintWorkingDirectory, // Prints the current directory.
        CreateFolder, // Creates a new folder.
        CreateFile, // Creates a new file.
        Remove, // Deletes a file or folder.
        Rename, // Renames a file or folder.
        Move, // Moves a file or folder.
        Copy, // Copies a file or folder.
        Navigate, // Changes the current directory.
        ListContent, // Lists contents of a folder.
        ReadFiles, // Views file content.
        FileEditor, // Edits file content.
        SearchFiles, // Searches for files.
        ShowHistory, // Displays access history.
        Restore, // Restore an item from the Recycle Bin.
        EmptyRecycleBin, // Empty the Recycle Bin permanently.
        ListRecycleBin, // List contents of the Recycle Bin.
        Exit, // Exits the application.
        Unknown // Represents an unrecognized command.
    }

    public class Program // Declares the main Program class.
    {
        // Converts a command string to its corresponding Commands enum value.
        private static Commands GetCommands(string command)
        {
            return command.ToLower() switch // Uses a switch expression for cleaner mapping.
            {
                "help" => Commands.Help, // Maps "help" to Commands.Help.
                "pwd" => Commands.PrintWorkingDirectory, // Maps "pwd" to Commands.PrintWorkingDirectory.
                "mkdir" => Commands.CreateFolder, // Maps "mkdir" to Commands.CreateFolder.
                "touch" => Commands.CreateFile, // Maps "touch" to Commands.CreateFile.
                "rm" => Commands.Remove, // Maps "rm" to Commands.Remove.
                "rename" => Commands.Rename, // Maps "rename" to Commands.Rename.
                "mv" => Commands.Move, // Maps "mv" to Commands.Move.
                "cp" => Commands.Copy, // Maps "cp" to Commands.Copy.
                "cd" => Commands.Navigate, // Maps "cd" to Commands.Navigate.
                "ls" => Commands.ListContent, // Maps "ls" to Commands.ListContent.
                "cat" => Commands.ReadFiles, // Maps "cat" to Commands.ReadFiles.
                "nano" => Commands.FileEditor, // Maps "nano" to Commands.FileEditor.
                "search" => Commands.SearchFiles, // Maps "search" to Commands.SearchFiles.
                "history" => Commands.ShowHistory, // Maps "history" to Commands.ShowHistory.
                "restore" => Commands.Restore, // Maps "restore" to Commands.Restore
                "emptyrb" => Commands.EmptyRecycleBin, // Maps "emptyrb" to Commands.EmptyRecycleBin
                "lsrb" => Commands.ListRecycleBin, // Maps "lsrb" to Commands.ListRecycleBin
                "exit" => Commands.Exit, // Maps "exit" to Commands.Exit.
                _ => Commands.Unknown // Default case for unknown commands.
            };
        }


        public static void Main(string[] args) // The entry point of the application.
        {
            FileManager fileManager = new FileManager(); // Creates a new instance of FileManager.
            Console.WriteLine("C# File Storage System\nAuthor: Peh Yan Bin"); // Prints welcome message and author.
            fileManager.PrintWorkingDirectory(); // Displays the initial working directory.

            while (true) // Enters an infinite loop for command input.
            {
                Console.Write($"\n{fileManager.CurrentFolder.GetFullPath()}> "); // Displays the current path as a prompt.
                string commandLine = Console.ReadLine()?.Trim(); // Reads a line of input from the console and trims whitespace.

                if (string.IsNullOrEmpty(commandLine)) // If the input is empty or just whitespace.
                {
                    continue; // Continues to the next loop iteration.
                }

                // Splits the command line into parts based on spaces, removing empty entries.
                string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Commands command = GetCommands(parts[0].ToLower()); // Gets the Commands enum for the first part (the command name).

                try
                {
                    switch (command) // Uses a switch statement to handle different commands.
                    {
                        case Commands.Help: // If the command is "help".
                            fileManager.DisplayHelp(); // Calls the DisplayHelp method.
                            break; // Exits the switch.
                        case Commands.PrintWorkingDirectory: // If the command is "pwd".
                            fileManager.PrintWorkingDirectory(); // Calls the PrintWorkingDirectory method.
                            break;
                        case Commands.CreateFolder: // If the command is "mkdir".
                            if (parts.Length > 1) // Checks if a folder name is provided.
                            {
                                fileManager.CreateFolder(parts[1]); // Calls CreateFolder with the specified name.
                            }
                            else
                            {
                                Console.WriteLine("Usage: mkdir <folder_name>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.CreateFile: // If the command is "touch".
                            if (parts.Length > 1) // Checks if a file name is provided.
                            {
                                // Joins remaining parts as content, if available.
                                string content = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";
                                fileManager.CreateFile(parts[1], content); // Calls CreateFile with name and content.
                            }
                            else
                            {
                                Console.WriteLine("Usage: touch <file_name> [content]"); // Prints usage instructions.
                            }
                            break;
                        case Commands.Remove: // If the command is "rm".
                            if (parts.Length > 1) // Checks if a path is provided.
                            {
                                fileManager.DeleteEntity(parts[1]); // Calls DeleteEntity with the specified path.
                            }
                            else
                            {
                                Console.WriteLine("Usage: rm <path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.Rename: // If the command is "rename".
                            if (parts.Length == 3) // Checks if both old path and new name are provided.
                            {
                                fileManager.RenameEntity(parts[1], parts[2]); // Calls RenameEntity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: rename <path> <new_name>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.Move: // If the command is "mv".
                            if (parts.Length == 3) // Checks if source and destination paths are provided.
                            {
                                fileManager.MoveEntity(parts[1], parts[2]); // Calls MoveEntity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: mv <source_path> <destination_path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.Copy: // If the command is "cp".
                            if (parts.Length == 3) // Checks if source and destination paths are provided.
                            {
                                fileManager.CopyEntity(parts[1], parts[2]); // Calls CopyEntity.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cp <source_path> <destination_path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.Navigate: // If the command is "cd".
                            if (parts.Length > 1) // Checks if a path is provided.
                            {
                                fileManager.NavigateTo(parts[1]); // Calls NavigateTo.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cd <path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.ListContent: // If the command is "ls".
                            bool detailed = parts.Contains("-l"); // Checks if the "-l" flag for detailed view is present.
                            string lsPath = null; // Initializes path for ls.
                            // Determines the path for ls command, handling cases with or without '-l' and explicit paths.
                            if (parts.Length > 1 && !parts[1].Equals("-l", StringComparison.OrdinalIgnoreCase))
                            {
                                lsPath = parts[1];
                            }
                            else if (parts.Length > 2 && parts[1].Equals("-l", StringComparison.OrdinalIgnoreCase))
                            {
                                lsPath = parts[2];
                            }
                            fileManager.ListContents(lsPath, detailed); // Calls ListContents.
                            break;
                        case Commands.ReadFiles: // If the command is "cat".
                            if (parts.Length > 1) // Checks if a file path is provided.
                            {
                                fileManager.ViewFile(parts[1]); // Calls ViewFile.
                            }
                            else
                            {
                                Console.WriteLine("Usage: cat <file_path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.FileEditor: // If the command is "nano".
                            if (parts.Length > 1) // Checks if a file path is provided.
                            {
                                fileManager.EditFile(parts[1]); // Calls EditFile.
                            }
                            else
                            {
                                Console.WriteLine("Usage: nano <file_path>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.SearchFiles: // If the command is "search".
                            if (parts.Length > 1) // Checks if a search term is provided.
                            {
                                string searchTerm = parts[1]; // Gets the search term.
                                fileManager.SearchFiles(searchTerm); // Calls SearchFiles.
                            }
                            else
                            {
                                Console.WriteLine("Usage: search <term>"); // Prints usage instructions.
                            }
                            break;
                        case Commands.ShowHistory: // If the command is "history".
                            fileManager.DisplayHistory(); // Calls DisplayHistory.
                            break;
                        case Commands.Restore: // New case for restore command
                            if (parts.Length > 1)
                            {
                                fileManager.RestoreEntity(parts[1]);
                            }
                            else
                            {
                                Console.WriteLine("Usage: restore <recycle_bin_path>");
                            }
                            break;
                        case Commands.EmptyRecycleBin: // New case for emptyrb command
                            fileManager.EmptyRecycleBin();
                            break;
                        case Commands.ListRecycleBin: // New case for lsrb command
                            bool detailedRb = parts.Contains("-l");
                            fileManager.ListRecycleBinContents(detailedRb);
                            break;
                        case Commands.Exit: // If the command is "exit".
                            Console.WriteLine("File System shutted down."); // Prints shutdown message.
                            return; // Exits the Main method, terminating the application.
                        default: // For any unrecognized command.
                            Console.WriteLine($"Error: Unknown command '{command}'. Type 'help' for available commands."); // Prints an error message.
                            break;
                    }
                }
                catch (Exception ex) // Catches any unhandled exceptions during command execution.
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Prints the error message.
                }
            }
        }
    }
}
