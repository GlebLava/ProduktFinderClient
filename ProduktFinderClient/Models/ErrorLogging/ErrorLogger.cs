using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ProduktFinderClient.Models.ErrorLogging
{
    class ErrorLogger
    {
        public static void LogError(Exception e, string keywordSearched)
        {
            MessageBox.Show("Es ist ein Fehler aufgetreten. Dieser wurde notiert. Entweder den versuchten Vorgang nochmal wiederholen oder das Program neustarten :)" +
                "\n ERROR MESSAGE:" + e.Message);

            string baseDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            baseDirPath = Path.Combine(baseDirPath, "ProduktFinder","ErrorLogs");

            Directory.CreateDirectory(baseDirPath);

            using (StreamWriter w = File.AppendText(Path.Combine(baseDirPath, "log.txt")))
            {
                w.WriteLine("Keyword searched: " + keywordSearched + "  " + e.Message + "  \n Stacktrace: " + e.StackTrace);
            }

            Console.WriteLine(baseDirPath);

        }

        public static void LogErrorHeaderMissing(Exception e, string path)
        {
            MessageBox.Show("In der gegebenen CSV Datei findet sich nicht die richtige Zeile" +
                "\n ERROR MESSAGE:" + e.Message + "\n" + "CSV Datei befindet sich bei: " + path);
           
            string baseDirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            baseDirPath = Path.Combine(baseDirPath, "ProduktFinder", "ErrorLogs");

            Directory.CreateDirectory(baseDirPath);

            using (StreamWriter w = File.AppendText(Path.Combine(baseDirPath, "log.txt")))
            {
                w.WriteLine("CSV Datei befindet sich bei:  " + path + "  " + e.Message + "  \n Stacktrace: " + e.StackTrace);
            }

            Console.WriteLine(baseDirPath);

        }
    }
}
