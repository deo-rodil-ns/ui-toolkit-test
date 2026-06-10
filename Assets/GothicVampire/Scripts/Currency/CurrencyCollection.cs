using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Currencies
{
    /// <summary>
    /// Represents a unique collection of currencies. Added currencies of the same type have their value appended to the existing type.
    /// </summary>
    public class CurrencyCollection : IReadOnlyCollection<Currency>
    {
        public bool AllowNegative { get; }
        
        public UnityEvent<CurrencyUpdatedArgs> EvtUpdated { get; } = new();
        public UnityEvent<CurrencyUpdatedArgs> EvtAdded { get; } = new();
        
        public UnityEvent<CurrencyUpdatedArgs> EvtDeducted { get; } = new();
        private readonly Dictionary<CurrencyType, Currency> _currencies = new();

        public CurrencyCollection(bool allowNegative = true)
        {
            AllowNegative = allowNegative;
        }

        public CurrencyCollection(IEnumerable<Currency> currencies, bool allowNegative = true)
        {
            AllowNegative = allowNegative;
            foreach (var currency in currencies)
            {
                Add(currency);
            }
        }
        
        public Currency? Get(CurrencyType type)
        {
            if (!_currencies.TryGetValue(type, out var currency)) return null;
            return currency;
        }

        public float GetValue(CurrencyType type)
        {
            return Get(type)?.Value ?? 0f;
        }
        
        public Currency Set(Currency currency)
        {
            if (!currency.IsValid) throw new ArgumentException("Currency is not valid");

            var previous = Get(currency.Type) ?? currency;
            _currencies[currency.Type] = AllowNegative
                ? currency.WithValue(Mathf.Min(currency.Value, currency.Max))
                : currency.WithValue(Mathf.Clamp(currency.Value, 0, currency.Max));
            
            EvtUpdated?.Invoke(new CurrencyUpdatedArgs
            {
                Currency = currency, 
                Previous = previous
            });

            return currency;
        }

        public Currency Set(CurrencyType type, float value)
        {
            // Preserve existing resource details
            var currency = Get(type)?.WithValue(value) ?? type.CreateCurrency(value);
            Set(currency);
            
            return currency;
        }

        public Currency SetMax(CurrencyType type, float max)
        {
            // Preserve existing resource details
            var currency = Get(type)?.WithMax(max) ?? type.CreateCurrency(0, max);
            Set(currency);
            
            return currency;
        }
        
        public Currency Add(Currency currency)
        {
            // Add to existing resource. Create a new resource if it doesn't exist yet.
            var source = Get(currency.Type) ?? currency.Type.CreateCurrency();
            var previous = source;
            source += currency;
            source = Set(source);
            
            EvtAdded?.Invoke(new CurrencyUpdatedArgs
            {
                Currency = source, 
                Previous = previous
            });

            return source;
        }

        public Currency Add(CurrencyType type, float value)
        {
            return Add(type.CreateCurrency(value));
        }
        
        public IReadOnlyCollection<Currency> Add(params Currency[] currencies)
        {
            return currencies.Select(Add).ToList();
        }

        public IReadOnlyCollection<Currency> Add(IReadOnlyCollection<Currency> currencies)
        {
            return currencies.Select(Add).ToList();
        }

        public Currency Deduct(Currency currency)
        {
            // Deduct from existing resource. Create a new resource if it doesn't exist yet.
            var source = Get(currency.Type) ?? currency.Type.CreateCurrency();
            var previous = source;
            source -= currency;
            source = Set(source);
            
            EvtDeducted?.Invoke(new CurrencyUpdatedArgs
            {
                Currency = source, 
                Previous = previous
            });

            return source;
        }

        public IReadOnlyCollection<Currency> Deduct(params Currency[] currencies)
        {
            return currencies.Select(Deduct).ToList();
        }

        public IReadOnlyCollection<Currency> Deduct(IReadOnlyCollection<Currency> currencies)
        {
            return currencies.Select(Deduct).ToList();
        }

        public void RemoveType(CurrencyType type)
        {
            _currencies.Remove(type);
        }

        public bool HasEnough(Currency currency)
        {
            return Get(currency.Type)?.Value >= currency.Value;
        }
        
        public bool HasEnough(IReadOnlyCollection<Currency> currencies)
        {
            return currencies.All(HasEnough);
        }

        public bool HasEnough(params Currency[] currencies)
        {
            return currencies.All(HasEnough);
        }

        public IEnumerator<Currency> GetEnumerator()
        {
            return _currencies.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _currencies.Count;
    }

    [System.Serializable]
    public struct CurrencyUpdatedArgs
    {
        public Currency Currency;
        public Currency Previous;

        public float Delta
        {
            get
            {
                if (Previous.Type != Currency.Type) return 0f;
                return Previous.Value - Currency.Value;
            }
        }
    }
}