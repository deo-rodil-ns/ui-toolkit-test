namespace Sylpheed.Math
{
    [System.Serializable]
    public class StackedModifier
    {
        public double Base;
        public AddMultModifier Modifier { get; set; }
        public FlatModifier Flat { get; set; }
        public double Value
        {
            get
            {
                double value = (Base * Modifier) + Flat;
                if (OnFinalizeValue != null) return OnFinalizeValue(value);
                else return value;
            }
        }

        public delegate double FinalizeValueDelegate(double value);
        /// <summary>
        /// Override this to filter and finalize the output value
        /// </summary>
        public FinalizeValueDelegate OnFinalizeValue;
    }

    [System.Serializable]
    public struct AddMultModifier
    {
        private AdditiveModifier add;
        private MultiplicativeModifier mult;

        public static implicit operator double(AddMultModifier s)
        {
            return s.add * s.mult;
        }

        public static AddMultModifier operator +(AddMultModifier s, double val)
        {
            s.add += val;
            return s;
        }

        public static AddMultModifier operator -(AddMultModifier s, double val)
        {
            return s + (val * -1.0);
        }

        public static AddMultModifier operator *(AddMultModifier s, double val)
        {
            s.mult *= val;
            return s;
        }

        public static AddMultModifier operator /(AddMultModifier s, double val)
        {
            return s * (1.0 / val);
        }
    }

    [System.Serializable]
    public struct AdditiveModifier
    {
        private double? value;
        private double Value
        {
            get => value ?? 1.0;
            set => this.value = value;
        }

        public static AdditiveModifier operator +(AdditiveModifier s, double val)
        {
            if (s.Value + val <= 0) throw new System.Exception("Cannot set modifier value to 0 or negative");
            return new AdditiveModifier { Value = s.Value + val };
        }

        public static AdditiveModifier operator -(AdditiveModifier s, double val)
        {
            return s + (val * -1.0);
        }

        public static implicit operator double(AdditiveModifier s)
        {
            return s.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    [System.Serializable]
    public struct MultiplicativeModifier
    {
        private double? value;
        private double Value
        {
            get => value ?? 1.0;
            set => this.value = value;
        }

        public static MultiplicativeModifier operator *(MultiplicativeModifier s, double val)
        {
            if (val <= 0) throw new System.Exception("Cannot stack a zero or negative value");

            return new MultiplicativeModifier { value = s.Value * val };
        }

        public static MultiplicativeModifier operator /(MultiplicativeModifier s, double val)
        {
            return s * (1.0 / val);
        }

        public static implicit operator double(MultiplicativeModifier modifier)
        {
            return modifier.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    [System.Serializable]
    public struct FlatModifier
    {
        public double value;

        public static FlatModifier operator +(FlatModifier s, double val)
        {
            return new FlatModifier { value = s.value + val };
        }

        public static FlatModifier operator -(FlatModifier s, double val)
        {
            return s + (val * -1.0);
        }

        public static implicit operator double(FlatModifier s)
        {
            return s.value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}