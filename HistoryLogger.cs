using System; // Provides fundamental classes and base types
using System.IO; // Provides types for reading and writing files and data streams
using System.Linq; // Provides classes and methods for LINQ
using System.Collections.Generic; // Provides interfaces and classes that define generic collections

namespace testonly.NewFolder // Declares a namespace for organizing related code
{
    public class HistoryLogger // Defines a class for logging file access history
    {
        private string _logFilePath; // Private field to store the path to the log file

        public string LogFilePath // Public property to get or set the log file path
        {
            get { return _logFilePath; } // Getter for _logFilePath
            set { _logFilePath = value; } // Setter for _logFilePath
        }

        public HistoryLogger(string logFileName = "file_access_history.log") // Constructor for HistoryLogger, with a default log file name
        {
            // Combines the base directory of the current application domain with the log file name to get the full path
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            if (!System.IO.File.Exists(_logFilePath)) // Checks if the log file does not exist
            {
                System.IO.File.Create(_logFilePath).Dispose(); // Creates the log file and immediately disposes the stream
            }
        }

        public void LogAccess(string filePath, DateTime accessTime) // Logs a file access event
        {
            try
            {
                // Appends the access time and file path to the log file, followed by a new line
                System.IO.File.AppendAllText(_logFilePath, $"{accessTime:yyyy-MM-dd HH:mm:ss} - Accessed: {filePath}{Environment.NewLine}");
            }
            catch (Exception ex) // Catches any exceptions that occur during logging
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints an error message
            }
        }

        public List<string> GetHistory() // Retrieves the entire access history from the log file
        {
            try
            {
                return System.IO.File.ReadAllLines(_logFilePath).ToList(); // Reads all lines from the log file and returns them as a list of strings
            }
            catch (Exception ex) // Catches any exceptions that occur during reading
            {
                Console.WriteLine($"Error: {ex.Message}"); // Prints an error message
                return new List<string>(); // Returns an empty list in case of an error
            }
        }
    }
}