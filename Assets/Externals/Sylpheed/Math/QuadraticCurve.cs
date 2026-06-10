namespace Sylpheed.Math
{
    [System.Serializable]
    public struct QuadraticCurve
    {
        public double a;
        public double b;
        public double c;

        public static double GetValue(double a, double b, double c, int time)
        {
            if (time <= 0) return 0;
            return (a * System.Math.Pow(time - 1, 2)) + (b * (time - 1)) + c;
        }

        public static double GetSeries(double a, double b, double c, int time)
        {
            if (time == 0) return 0;
            double sumA = a * ((2 * System.Math.Pow(time - 1, 3)) + (3 * System.Math.Pow(time - 1, 2)) + time - 1) / 6.0;
            double sumB = (time - 1) * (b * time) / 2;
            double sumC = c * time;

            return sumA + sumB + sumC;
        }

        public static double GetSeries(double a, double b, double c, int startTime, int steps)
        {
            double startSeries = GetSeries(a, b, c, startTime);
            double endSeries = GetSeries(a, b, c, startTime + steps);

            return endSeries - startSeries;
        }

        public double GetValue(int time)
        {
            return GetValue(a, b, c, time);
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
        /// <param name="steps"></param>
        /// <returns></returns>
        public double GetSeries(int steps)
        {
            return GetSeries(a, b, c, steps);
        }

        /// <summary>
        /// Compute for the sum of the values in the curve
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public double GetSeries(int startTime, int steps)
        {
            return GetSeries(a, b, c, startTime, steps);
        }
    }
}
