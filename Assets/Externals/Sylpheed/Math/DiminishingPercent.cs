namespace Sylpheed.Math
{
    [System.Serializable]
    public struct DiminishingPercent
    {
        public double Coefficient;
        public double MinValue;
        public double MaxValue;

        public float GetValue(double sourceValue)
        {
            return Math.ComputeDiminishingChance(sourceValue, Coefficient, MinValue, MaxValue);
        }
    }
}
