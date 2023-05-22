using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ProduktFinderClient.Models;

public static class LoadSaveSystem
{
    //Have to be on top
    private static readonly string savingDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProduktFinder");
    private static readonly string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProduktFinder", "ConfigFile.json");
    private static readonly string authKeyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ProduktFinder", "authKey.pk");


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

    static void SaveData(ConfigData data)
    {
        File.WriteAllText(configFilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
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
        SaveData(configData);
    }

    public static void SaveMostUsedKeywordsModules()
    {
        File.WriteAllText(Path.Combine(savingDirPath, nameof(bedarfMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(bedarfMostUsedKeywordsModule));
        File.WriteAllText(Path.Combine(savingDirPath, nameof(hArtikelNrMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(hArtikelNrMostUsedKeywordsModule));
        File.WriteAllText(Path.Combine(savingDirPath, nameof(hcsArtikelNrMostUsedKeywordsModule) + ".json"), JsonSerializer.Serialize(hcsArtikelNrMostUsedKeywordsModule));
    }

    public static HashSet<ModuleType> LoadCheckedSuppliers()
    {
        configData = LoadData();
        return configData.SuppliersChecked;
    }

    public static void SaveCheckedSuppliers(HashSet<ModuleType> checkedSuppliers)
    {
        configData.SuppliersChecked = checkedSuppliers;
        SaveData(configData);
    }

    public static void SaveCheckedSuppliers(ICollection<CheckableStringObject> checkedSuppliers)
    {
        HashSet<ModuleType> checkedSuppliersSet = new HashSet<ModuleType>();
        foreach (CheckableStringObject checkableString in checkedSuppliers)
        {
            if (checkableString.IsChecked)
            {
                ModuleTranslations.ModulesTranslation.TryGetKey(checkableString.AttributeName, out ModuleType moduleType);
                checkedSuppliersSet.Add(moduleType);
            }
        }
        SaveCheckedSuppliers(checkedSuppliersSet);
    }

    public static void SaveSearchedKeyWord(string keyWord)
    {
        configData.LastSearchedForKeyWord = keyWord;
        SaveData(configData);
    }

    public static string LoadLastSearchedKeyWord()
    {
        configData = LoadData();
        return configData.LastSearchedForKeyWord;
    }

    public static OptionsConfigData LoadOptionsConfig()
    {
        configData = LoadData();
        return configData.OptionsConfigData;
    }

    public static void SaveOptionsConfig(OptionsConfigData data)
    {
        configData.OptionsConfigData = data;
        SaveData(configData);
    }

    public static void SaveAuthKey(string authKey)
    {
        File.WriteAllText(authKeyFilePath, authKey);
    }

    public static string LoadAuthKey()
    {
        if (!File.Exists(authKeyFilePath))
        {
            SaveAuthKey("");
        }

        return File.ReadAllText(authKeyFilePath);
    }

}


