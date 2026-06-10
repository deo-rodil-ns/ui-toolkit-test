using System;
using UnityEngine;

namespace GothicVampire.Currencies
{
    [Serializable]
    public struct Currency
    {
        [SerializeField] private CurrencyType _type;
        [SerializeField] private float _value;
        [SerializeField] private float _max;
        
        public CurrencyType Type => _type;
        public float Value => _value;
        public float Max => _max <= 0 ? float.PositiveInfinity : _max;
        public bool HasMax => _max > 0 && !float.IsPositiveInfinity(_max);
        public bool IsValid => Type != null;

        public Currency(CurrencyType type, float value, float max = float.PositiveInfinity)
        {
            _type = type;
            _value = value;
            _max = max;
        }

        public Currency WithValue(float value)
        {
            return new Currency(_type, value, Max);
        }

        public Currency WithMax(float max)
        {
            return new Currency(_type, _value, max);
        }
        
        public static Currency operator +(Currency a, float value)
        {
            var newValue = Mathf.Min(a.Value + value, a.Max);
            return new Currency(a.Type, newValue, a.Max);
        }
        
        public static Currency operator -(Currency a, float value)
        {
            var newValue = Mathf.Min(a.Value - value, a.Max);
            return new Currency(a.Type, newValue, a.Max);
        }

        public static Currency operator +(Currency a, Currency b)
        {
            if (a.Type != b.Type) throw new ArgumentException("Resource type mismatch");
            return a + b.Value;
        }
        
        public static Currency operator -(Currency a, Currency b)
        {
            if (a.Type != b.Type) throw new ArgumentException("Resource type mismatch");
            return a - b.Value;
        }

        public static Currency operator *(Currency a, float value)
        {
            var newValue = Mathf.Min(a.Value * value, a.Max);
            return new Currency(a.Type, newValue, a.Max);
        }

        public override string ToString()
        {
            return $"{_type?.DisplayName ?? "Null"}: {_value}";
        }
        
        public string ToFormattedString()
        {
            return $"{_value} {_type.DisplayName}";
        }
    }
}