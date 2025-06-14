using System; // Imports the System namespace, providing fundamental classes and base types.
using System.IO; // Imports the System.IO namespace, for input/output operations like file and directory manipulation.
using System.Linq; // Imports the System.Linq namespace, for LINQ (Language Integrated Query) operations.
using System.Collections.Generic; // Imports the System.Collections.Generic namespace, for generic collection types like List.

namespace FileStorageSystem // Defines the namespace for the file storage system.
{
    public class HistoryLogger // Declares the public class HistoryLogger, responsible for logging file access.
    {
        private string _logFilePath; // Private field to store the path to the log file.

        public string LogFilePath // Public property to get or set the log file path.
        {
            get { return _logFilePath; } // Getter for LogFilePath.
            set { _logFilePath = value; } // Setter for LogFilePath.
        }

        public HistoryLogger(string logFileName = "file_access_history.log") // Constructor for HistoryLogger, with a default log file name.
        {
            // Combines the application's base directory with the log file name to create the full log file path.
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            if (!System.IO.File.Exists(_logFilePath)) // Checks if the log file does not exist.
            {
                System.IO.File.Create(_logFilePath).Dispose(); // Creates the log file if it doesn't exist, and disposes the file stream immediately.
            }
        }

        // Logs an access event to the history file.
        public void LogAccess(string filePath, DateTime accessTime)
        {
            try
            {
                // Appends a formatted log entry (timestamp - Accessed: filePath) to the log file.
                System.IO.File.AppendAllText(_logFilePath, $"{accessTime:yyyy-MM-dd HH:mm:ss} - Accessed: {filePath}{Environment.NewLine}");
            }
            catch (Exception ex) // Catches any exceptions during logging.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message to the console.
            }
        }

        // Retrieves all entries from the history log file.
        public List<string> GetHistory()
        {
            try
            {
                return System.IO.File.ReadAllLines(_logFilePath).ToList(); // Reads all lines from the log file and returns them as a List of strings.
            }
            catch (Exception ex) // Catches any exceptions during reading the history.
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints the error message to the console.
                return new List<string>(); // Returns an empty list in case of an error.
            }
        }
    }
}