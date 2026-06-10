using UnityEngine.Assertions;

namespace Sylpheed.Math
{
    [System.Serializable]
    public struct LinearExponentialCurve
    {
        public double Base;
        public double LinearGrowth;
        public double ExponentialGrowth;

        public double GetValue(int time)
        {
            if (time <= 0) return 0;

            // Cut off base value to isolate the growth
            double linear = LinearGrowth * (time - 1);
            double exponential = 0;
            if (ExponentialGrowth != 0) exponential = ExponentialCurve.GetExponentialValue(Base, ExponentialGrowth, time) - Base;

            // Merge the values
            double val = Base + linear + exponential;

            if (val == double.PositiveInfinity) return double.MaxValue;
            if (val == double.NegativeInfinity) return double.MinValue;

            return System.Math.Max(0, val);
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
        /// Compute for the sum of values in the curve
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public double GetSeries(int startTime, int steps)
        {
            Assert.IsTrue(startTime >= 0);
            Assert.IsTrue(steps >= 0);

            if (steps == 0) return GetValue(startTime);

            // Compute series for both curves
            double linearPart = LinearCurve.ArithmeticSeries(LinearGrowth, LinearGrowth, startTime, steps);
            double exponentialPart = 0;
            if (ExponentialGrowth != 0) exponentialPart = ExponentialCurve.GeometricSeries(Base, ExponentialGrowth, startTime, steps);

            // Merge the values
            return linearPart + exponentialPart;
        }
    }
}
