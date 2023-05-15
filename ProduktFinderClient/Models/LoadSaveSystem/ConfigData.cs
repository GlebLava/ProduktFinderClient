using ProduktFinderClient.DataTypes;
using System.Collections.Generic;

namespace ProduktFinderClient.Models;

public class ConfigData
{
    public string LastUsedSaveFile { get; set; } = "";
    public HashSet<ModuleType> SuppliersChecked { get; set; } = new HashSet<ModuleType>();
    public string LastSearchedForKeyWord { get; set; } = "bc847c";

    public OptionsConfigData OptionsConfigData { get; set; } = new();

}

public class OptionsConfigData
{
    public int ResultsInSearchPerAPI { get; set; } = 10;
    public List<CheckableStringObject> AttributesChecked { get; set; } = new List<CheckableStringObject>();
    public List<CheckableStringObject> SortsChecked { get; set; } = new List<CheckableStringObject>();
}

