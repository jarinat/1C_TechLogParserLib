using System;
using System.Text.RegularExpressions;
using System.IO;

namespace _1C_TechLogParserLib
{
    public class LogFilesWalker
    {
        private FileInfo[] logFiles;
        private int nextFileIndex = 0;
        private const string logFilesPattern = @"rphost_\d+\\\d{8}\.log";

        public LogFilesWalker(string lp)
        {
            DirectoryInfo dir = new DirectoryInfo(@"" + lp);
            logFiles = dir.GetFiles("*.log", SearchOption.AllDirectories);
        }

        public string GetNextFilePath()
        {
            string nextFilePath = String.Empty;

            while (nextFileIndex < logFiles.Length)
            {
                nextFileIndex++;

                Match m = Regex.Match(logFiles[nextFileIndex - 1].FullName, logFilesPattern);

                if (m.Success)
                {
                    nextFilePath = logFiles[nextFileIndex - 1].FullName;
                    break;
                }
            }
            
            return nextFilePath;
        }
        
    }
}
