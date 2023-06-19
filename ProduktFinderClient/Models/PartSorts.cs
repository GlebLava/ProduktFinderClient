using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models
{
    public static class PartSorts
    {
        private static readonly Dictionary<string, Action<List<Part>>> _sortNameToSortMethod;

        static PartSorts()
        {
            _sortNameToSortMethod = new();
            _sortNameToSortMethod["Verfügbarkeit aufsteigend"] = SortAvailableAscend;
            _sortNameToSortMethod["Verfügbarkeit absteigend"] = SortAvailableDescend;
            _sortNameToSortMethod["Preis Ø aufsteigend"] = SortAveragePriceAscend;
            _sortNameToSortMethod["Preis Ø absteigend"] = SortAveragePriceDescend;
        }

        public static List<string> GetSortMethodStringTranslations()
        {
            return _sortNameToSortMethod.Keys.ToList();
        }

        public static void Sort(ref List<Part> parts, string filterName)
        {
            _sortNameToSortMethod[filterName](parts);
        }

        private static void SortAvailableAscend(List<Part> parts)
        {
            SortInPlace((p1, p2) => p1.AmountInStock < p2.AmountInStock, ref parts);
        }

        private static void SortAvailableDescend(List<Part> parts)
        {
            SortInPlace((p1, p2) => p1.AmountInStock > p2.AmountInStock, ref parts);
        }
        private static void SortAveragePriceAscend(List<Part> parts)
        {
            SortInPlace((p1, p2) => GetPricesAverage(p1) > GetPricesAverage(p2), ref parts);
        }

        private static void SortAveragePriceDescend(List<Part> parts)
        {
            SortInPlace((p1, p2) => GetPricesAverage(p1) < GetPricesAverage(p2), ref parts);
        }

        private static float GetPricesAverage(Part part)
        {
            float average = 0.0f;

            foreach (var price in part.Prices)
            {
                average += price.PricePerPiece;
            }

            if (part.Prices.Count > 0) average /= part.Prices.Count;
            return average;
        }



        private static void SortInPlace(Func<Part, Part, bool> predicate, ref List<Part> list)
        {
            int n = list.Count;
            bool swapped;

            do
            {
                swapped = false;
                for (int i = 1; i < n; i++)
                {
                    if (predicate(list[i - 1], list[i]))
                    {
                        // Swap elements
                        var temp = list[i - 1];
                        list[i - 1] = list[i];
                        list[i] = temp;
                        swapped = true;
                    }
                }
                n--;
            } while (swapped);
        }
    }
}
