using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models;

public enum ModuleType
{
    MOUSER = 0,
    FARNELL = 1,
    FUTURE = 2,
    MYARROW = 3,
    SCHUKAT = 4,
    REICHELT = 5,
    DIGI_KEY = 6,
}


public class ModuleTranslations
{
    public HashSet<ModuleType> ModulesToSearchWith { get; set; } = new HashSet<ModuleType>();

    [NonSerialized]
    public static readonly BidirectionalDictionary<ModuleType, string> ModulesTranslation;

    // Just so we can init ModulesTranslation
    static ModuleTranslations()
    {
        ModulesTranslation = new BidirectionalDictionary<ModuleType, string>();
        ModulesTranslation.Add(ModuleType.MOUSER, "Mouser");
        ModulesTranslation.Add(ModuleType.FARNELL, "Farnell");
        ModulesTranslation.Add(ModuleType.FUTURE, "Future");
        ModulesTranslation.Add(ModuleType.MYARROW, "MyArrow");
        ModulesTranslation.Add(ModuleType.SCHUKAT, "Schukat");
        ModulesTranslation.Add(ModuleType.REICHELT, "Reichelt");
        ModulesTranslation.Add(ModuleType.DIGI_KEY, "DigiKey");
    }

    public static List<string> GetModuleNamesList()
    {
        List<string> list = new();
        foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
        {
            ModulesTranslation.TryGetValue(moduleType, out string moduleString);
            list.Add(moduleString);
        }

        return list;
    }

}
