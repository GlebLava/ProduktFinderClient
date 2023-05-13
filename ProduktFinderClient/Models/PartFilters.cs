using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models
{
    public class PartFilters
    {
        public static void FilterAvailableLessThen(List<Part> parts, int lessThenAmount)
        {
            Filter((p) => p.AmountInStock <= lessThenAmount, parts);
        }

        public static void FilterAvailableMoreThen(List<Part> parts, int moreThenAmount)
        {
            Filter((p) => p.AmountInStock >= moreThenAmount, parts);
        }

        public static void FilterLessThenPriceAt(List<Part> parts, float price, int at)
        {
            Filter((p) =>
            {
                var priceEntry = p.Prices.Find(price => price.FromAmount >= at);
                if (priceEntry != null)
                {
                    if (priceEntry.PricePerPiece <= price)
                        return true;
                }
                return false;
            }, parts);
        }

        private static void Filter(Func<Part, bool> predicate, List<Part> parts)
        {
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                if (!predicate(parts[i]))
                {
                    parts.RemoveAt(i);
                }
            }
        }

    }
}
