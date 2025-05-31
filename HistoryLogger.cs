using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace testonly.NewFolder
{
    public class HistoryLogger
    {
        private string _logFilePath;

        public string LogFilePath
        {
            get { return _logFilePath; }
            set { _logFilePath = value; }
        }

        public HistoryLogger(string logFileName = "file_access_history.log")
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            if (!System.IO.File.Exists(_logFilePath))
            {
                System.IO.File.Create(_logFilePath).Dispose();
            }
        }

        public void LogAccess(string filePath, DateTime accessTime)
        {
            try
            {
                System.IO.File.AppendAllText(_logFilePath, $"{accessTime:yyyy-MM-dd HH:mm:ss} - Accessed: {filePath}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public List<string> GetHistory()
        {
            try
            {
                return System.IO.File.ReadAllLines(_logFilePath).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<string>();
            }
        }
    }
}