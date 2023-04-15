using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ProduktFinderClient.Models
{
    public static class LoadSaveSystem
    {   
        //Have to be on top
        private static string savingDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProduktFinder");
        private static string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProduktFinder", "ConfigFile.json");


        public static ConfigData configData = LoadData();

        public static MostUsedKeywordsModule bedarfMostUsedKeywordsModule = LoadMostUsedKeywordsModule(nameof(bedarfMostUsedKeywordsModule));
        public static MostUsedKeywordsModule hArtikelNrMostUsedKeywordsModule = LoadMostUsedKeywordsModule(nameof(hArtikelNrMostUsedKeywordsModule));
        public static MostUsedKeywordsModule hcsArtikelNrMostUsedKeywordsModule = LoadMostUsedKeywordsModule(nameof(hcsArtikelNrMostUsedKeywordsModule));

        static ConfigData LoadData()
        {
            ConfigData data;
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appdataPath = Path.Combine(appdataPath, "ProduktFinder");

            Directory.CreateDirectory(appdataPath);

            appdataPath = Path.Combine(appdataPath, "ConfigFile.json");

            if (!File.Exists(appdataPath))
            {
                data = new ConfigData();
                File.WriteAllText(appdataPath, JsonSerializer.Serialize(data));
            }
            else
            {

                string jsonConfigData = File.ReadAllText(appdataPath);
                data = JsonSerializer.Deserialize<ConfigData>(jsonConfigData)!;
                
                if (data == null)
                {
                    data = new ConfigData();
                    File.WriteAllText(appdataPath, JsonSerializer.Serialize(data));
                }

            }

            return data;
        }

        public static MostUsedKeywordsModule LoadMostUsedKeywordsModule(string fieldName)
        {
            string fullPath = Path.Combine(savingDirPath, fieldName + ".json");
            if (!File.Exists(fullPath))
            {
                //noch kein dir
                MostUsedKeywordsModule mostUsedKeywordsModule = new MostUsedKeywordsModule();
                File.WriteAllText(fullPath, JsonSerializer.Serialize(mostUsedKeywordsModule));
                return mostUsedKeywordsModule;
            }
            else
            {
                //deserialize
                MostUsedKeywordsModule mostUsedKeywordsModule = JsonSerializer.Deserialize<MostUsedKeywordsModule>(File.ReadAllText(fullPath));
                if (mostUsedKeywordsModule == null)
                {
                    File.Delete(fullPath);
                    return LoadMostUsedKeywordsModule(fieldName);
                }
                return mostUsedKeywordsModule;
            }
        }

        public static void SaveLastUsedSaveFile(string LastUsedSaveFile)
        {
            configData.LastUsedSaveFile = LastUsedSaveFile;
            File.WriteAllText(configFilePath, JsonSerializer.Serialize(configData));
        }

        public static void SaveMostUsedKeywordsModules()
        {
            File.WriteAllText(Path.Combine(savingDirPath, nameof(bedarfMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(bedarfMostUsedKeywordsModule));
            File.WriteAllText(Path.Combine(savingDirPath, nameof(hArtikelNrMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(hArtikelNrMostUsedKeywordsModule));
            File.WriteAllText(Path.Combine(savingDirPath, nameof(hcsArtikelNrMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(hcsArtikelNrMostUsedKeywordsModule));
        }
    }


    public class ConfigData
    {
        public string LastUsedSaveFile { get; set; }
    }


}
