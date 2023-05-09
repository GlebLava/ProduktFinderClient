using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models
{
    public class Filters
    {
        private static readonly Dictionary<string, Action<List<Part>>> _filterNameToFilterMethod;

        static Filters()
        {
            _filterNameToFilterMethod = new();
            _filterNameToFilterMethod["Verfügbar > 0"] = FilterAvailable;
        }


        public static void FilterAvailable(List<Part> parts)
        {
            parts = parts.Where(x => x.AmountInStock > 0).ToList();
        }

        public static List<string> GetFilterMethodStringTranslations()
        {
            return _filterNameToFilterMethod.Keys.ToList();
        }

        public static void Filter(string filterName, ref List<Part> parts)
        {
            _filterNameToFilterMethod[filterName](parts);
        }

       
    }
}
