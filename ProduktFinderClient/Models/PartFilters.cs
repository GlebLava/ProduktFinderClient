using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models
{
    public class PartFilters
    {
        private static readonly Dictionary<string, Action<List<Part>, object?>> _filterNameToFilterMethod;

        static PartFilters()
        {
            _filterNameToFilterMethod = new();
            _filterNameToFilterMethod["Mehr als 0 Verfügbar"] = FilterAvailable;
        }
        public static List<string> GetFilterMethodStringTranslations()
        {
            return _filterNameToFilterMethod.Keys.ToList();
        }

        public static void Filter(ref List<Part> parts, string filterName, object? param = null)
        {
            _filterNameToFilterMethod[filterName](parts, param);
        }

        private static void FilterAvailable(List<Part> parts, object? param)
        {
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (parts[i].AmountInStock <= 0)
                {
                    parts.RemoveAt(i);
                }
            }
        }



    }
}
