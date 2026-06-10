using UnityEngine.Assertions;

namespace Sylpheed.Math
{
    [System.Serializable]
    public struct ExponentialCurve
    {
        public double Base;
        public double Growth;

        public static double GetExponentialValue(double baseValue, double growth, int time)
        {
            if (time <= 0) return 0;
            return baseValue * System.Math.Pow(growth, time - 1);
        }

        public static double GeometricSeries(double baseValue, double growth, int startTime, int steps)
        {
            Assert.IsTrue(startTime >= 0);
            Assert.IsTrue(steps >= 0);
            if (startTime == 0) return 0;

            double startTerm = GetExponentialValue(baseValue, growth, startTime);
            return startTerm * (1 - System.Math.Pow(growth, steps + 1)) / (1.0 - growth);
        }

        public double GetValue(int time)
        {
            return GetExponentialValue(Base, Growth, time);
        }

        public double GetCeilValue(int time)
        {
            double val = GetValue(time);
            return System.Math.Ceiling(val);
        }

        public double GetFloorValue(int time)
        {
            double val = GetValue(time);
            return System.Math.Floor(val);
        }

        public int GetIntValue(int time)
        {
            return (int)GetValue(time);
        }

        public float GetFloatValue(int time)
        {
            return (float)GetValue(time);
        }

        public double GetRoundedValue(int time, int decimals)
        {
            double val = GetValue(time);
            return System.Math.Round(val, decimals);
        }

        /// <summary>
        /// Compute for the sum of the values in the curve
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public double GetSeries(int startTime, int steps)
        {
            return GeometricSeries(Base, Growth, startTime, steps);
        }
    }
}
