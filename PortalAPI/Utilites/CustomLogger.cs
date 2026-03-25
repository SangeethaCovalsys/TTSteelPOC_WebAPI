using System;
using System.IO;
namespace PortalAPI.Utilites
{
    public class CustomLogger 
    {
        private readonly string logFilePath;

        public CustomLogger(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public void LogError(string message)
        {
            using (StreamWriter writer = File.AppendText(logFilePath))
            {
                writer.WriteLine($"{DateTime.Now}: ERROR - {message}");
            }
        }

        public void LogInformation(string message)
        {
            // Implement logging of information messages if needed
        }
    }
}
