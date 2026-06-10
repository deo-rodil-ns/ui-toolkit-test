using System;

namespace Sylpheed.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string WithHighlight(this string value, string highlighted)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(highlighted))
                return value;

            string valueLower = value.ToLower();
            string highlightedLower = highlighted.ToLower();

            // Get index of the highlighted string
            int startIndex = valueLower.IndexOf(highlightedLower);
            if (startIndex < 0) return value;

            // Highlight substring
            string sub = value.Substring(startIndex, highlighted.Length);
            return value.Replace(sub, $"<mark>{sub}</mark>");
        }
    }
}