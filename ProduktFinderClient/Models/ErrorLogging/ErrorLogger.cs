using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace ProduktFinderClient.Models.ErrorLogging
{
    class ErrorLogger
    {
        private static readonly List<string> logNames = new() { "log1.txt", "log2.txt", "log3.txt" };


        public static void LogError(Exception e, string keywordSearched)
        {
            MessageBox.Show("Es ist ein Fehler aufgetreten. Dieser wurde notiert. Entweder den versuchten Vorgang nochmal wiederholen oder das Program neustarten :)" +
                "\n ERROR MESSAGE:" + e.Message);

            WriteToErrorLogFile($"\nTime: {DateTime.Parse(DateTime.Now.ToString())}\nKeyword searched: {keywordSearched}\nErrorMessage:{e.Message}\n Stacktrace:\n{e.StackTrace}");
        }

        public static void LogErrorHeaderMissing(Exception e, string path)
        {
            MessageBox.Show("In der gegebenen CSV Datei findet sich nicht die richtige Zeile" +
                "\n ERROR MESSAGE:" + e.Message + "\n" + "CSV Datei befindet sich bei: " + path);

            WriteToErrorLogFile("CSV Datei befindet sich bei:  " + path + "  " + e.Message + "  \n Stacktrace: " + e.StackTrace);
        }

        private static void WriteToErrorLogFile(string text)
        {
            string baseDirPath = GetErrorLogDir();
            Directory.CreateDirectory(baseDirPath);

            string current = GetCurrentFileInUse();

            if (IsFileTooLarge(current))
            {
                current = GetNextFileInRotation(current);
                // The next file to be used always gets erased
                File.WriteAllText(Path.Combine(baseDirPath, current), " ");
            }
            SaveCurrentFileInUseToConfig(current);

            using (StreamWriter w = File.AppendText(Path.Combine(baseDirPath, current)))
            {
                w.WriteLine(text);
            }

            Console.WriteLine(baseDirPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Not the full path, only the file name</param>
        private static bool IsFileTooLarge(string fileName)
        {
            string filePath = Path.Combine(GetErrorLogDir(), fileName);

            if (!File.Exists(filePath))
                return true;

            string s = File.ReadAllText(filePath);
            return s.Length > 100_000;
        }

        private static string GetErrorLogDir()
        {
            string baseDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            baseDirPath = Path.Combine(baseDirPath, "ProduktFinder", "ErrorLogs");
            return baseDirPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Not the full path, only the file name</param>
        private static void SaveCurrentFileInUseToConfig(string currentfileNameInUse)
        {
            string configFile = Path.Combine(GetErrorLogDir(), "logConfig.json");
            LogConfig logConfig = new LogConfig();
            logConfig.CurrentFileInUse = currentfileNameInUse;
            File.WriteAllText(configFile, JsonSerializer.Serialize(logConfig));
        }

        private static string GetNextFileInRotation(string nameOfCurrentFile)
        {
            int indexOfCurrent = logNames.IndexOf(nameOfCurrentFile);

            if (indexOfCurrent == -1)
                return logNames[0];

            return logNames[(indexOfCurrent + 1) % logNames.Count];
        }

        private static string GetCurrentFileInUse()
        {
            string configFile = Path.Combine(GetErrorLogDir(), "logConfig.json");

            if (File.Exists(configFile))
            {
                LogConfig? logConfig = JsonSerializer.Deserialize<LogConfig>(File.ReadAllText(configFile));
                if (logConfig is null)
                    return logNames[0];

                if (logConfig.CurrentFileInUse == null || logConfig.CurrentFileInUse == "")
                    return logNames[0];

                return logConfig.CurrentFileInUse;
            }

            return logNames[0];
        }
    }

    class LogConfig
    {
        public string CurrentFileInUse { get; set; } = "";
    }
}
