using System;

namespace Sylpheed.Extensions
{
    public static class SingleExtensions
    {
        public static bool IsValid(this Single value)
        {
            if (Single.IsInfinity(value)) return false;
            if (Single.IsNaN(value)) return false;

            return true;
        }

        public static string ToStringWithPrefix(this Single value, string format = "")
        {
            if (value > 0) return $"+{value.ToString(format)}";
            return value.ToString(format);
        }
    }
}