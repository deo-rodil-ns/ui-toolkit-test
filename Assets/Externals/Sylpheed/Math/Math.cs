using UnityEngine;
using UnityEngine.Assertions;

namespace Sylpheed.Math
{
    public static class Math
    {
        public static float ComputeDiminishingChance(double value, double coefficient, double minValue = 0, double maxValue = 1.0)
        {
            Assert.IsTrue(maxValue >= minValue, "Min value cannot be higher than max value");

            double rate = value / (value + coefficient);
            rate *= maxValue - minValue;
            rate += minValue;

            return (float)rate;
        }

        public static double Lerp(this double sourceValue, double targetValue, float t)
        {
            // Clamp t
            t = System.Math.Max(t, 0);
            t = System.Math.Min(t, 1.0f);
        
            double delta = targetValue - sourceValue;
            return sourceValue + (delta * t);
        }
        
        public static bool Approximately(this double a, double b)
        {
            return System.Math.Abs(a - b) < 0.0000001; 
        }

        public static double Clamp(double value, double min, double max)
        {
            Assert.IsTrue(max >= min, "Min value should be lower or equal to max value");

            double result = value;
            if (value.CompareTo(max) > 0) result = max;
            if (value.CompareTo(min) < 0) result = min;

            return result;
        }

        public static float KRDiminishing(float value, float softCap, float hardCap = float.PositiveInfinity, float conversionRate = 0.01f)
        {
            // Return value immediately since it won't diminish
            if (value <= softCap) return value;

            // Compute for number of intervals reached. The first interval isn't counted
            int numIntervals = (int)System.Math.Max(System.Math.Ceiling(value / softCap), 0);

            // Geometric series on values past the interval
            float series = softCap * (2 * (1 - (1 / Mathf.Pow(2, numIntervals))));

            // Compute for the excess value
            float excess = (value % softCap) / Mathf.Pow(2, numIntervals);

            // Hard cap
            float result = Mathf.Min(hardCap, series + excess);

            return result * conversionRate;
        }
    }
}
