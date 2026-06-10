using UnityEngine.Assertions;

namespace Sylpheed.Math
{
    [System.Serializable]
    public struct LinearCurve
    {
        public double Base;
        public double Growth;

        public static double GetLinearValue(double baseValue, double growth, int time)
        {
            if (time <= 0) return 0;
            return baseValue + (growth * (time - 1));
        }

        public static double ArithmeticSeries(double baseValue, double growth, int startTime, int steps)
        {
            Assert.IsTrue(startTime >= 0);
            Assert.IsTrue(steps >= 0);
            if (startTime == 0) return 0;

            double startTerm = GetLinearValue(baseValue, growth, startTime);
            if (steps == 0) return startTerm;

            double endTerm = GetLinearValue(baseValue, growth, startTime + steps - 1);

            return steps * (startTerm + endTerm) / 2.0;
        }

        public double GetValue(int time)
        {
            return GetLinearValue(Base, Growth, time);
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
            return ArithmeticSeries(Base, Growth, startTime, steps);
        }
    }
}
