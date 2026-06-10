using System.Collections.Generic;
using System.Linq;

namespace GothicVampire.Currencies
{
    public static class CurrencyExtension
    {
        public static IReadOnlyCollection<Currency> Concat(this IReadOnlyCollection<Currency> first, IReadOnlyCollection<Currency> second)
        {
            var combined = first.ToList();

            foreach (var toAdd in second)
            {
                // Add to existing, else create a new entry
                var existing = combined.FirstOrDefault(c => c.Type == toAdd.Type);
                if (!existing.IsValid) 
                    combined.Add(toAdd);
                else
                {
                    // Remove previous, then add the updated currency to the list
                    combined.RemoveAll(c => c.Type == toAdd.Type);
                    combined.Add(existing + toAdd);
                }
            }

            return combined;
        }
        
        public static IReadOnlyCollection<Currency> Collate(this IEnumerable<Currency> currencies)
        {
            var collection = new CurrencyCollection(currencies);
            return collection;
        }

        public static IReadOnlyCollection<Currency> Separate(this IEnumerable<Currency> currencies, IReadOnlyCollection<Currency> toSeparate)
        {
            var collection = new CurrencyCollection(currencies);
            collection.Deduct(toSeparate);
            return collection;
        }
        
        public static string FormatToString(this IEnumerable<Currency> currencies, string separator = ", ")
        {
            return string.Join(separator, currencies.Select(c => c.ToString()));
        }
    }
}