using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Currencies
{
    public sealed class Wallet : MonoBehaviour, IFactionService
    {
        #region Inspector
        [SerializeField] private CurrencyCollectionAsset _startingCurrencies;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<Wallet> _evtUpdated;
        [SerializeField] private UnityEvent<WalletCurrencyUpdatedArgs> _evtAdded;
        [SerializeField] private UnityEvent<WalletCurrencyUpdatedArgs> _evtDeducted;
        #endregion
        
        public IReadOnlyCollection<Currency> Currencies => _collection;
        
        /// <summary>
        /// Called on LateUpdate once when at least one currency is changed.
        /// </summary>
        public UnityEvent<Wallet> EvtUpdated => _evtUpdated;
        public UnityEvent<WalletCurrencyUpdatedArgs> EvtAdded => _evtAdded;
        public UnityEvent<WalletCurrencyUpdatedArgs> EvtDeducted => _evtDeducted;
        
        private readonly CurrencyCollection _collection = new(false);
        private bool _wasUpdated;
        
        #region IFactionService
        public Faction Faction { get; set; }
        void IFactionService.OnFactionInitialize(Faction faction) { }
        #endregion

        private void Awake()
        {
            if (_startingCurrencies != null) _collection.Add(_startingCurrencies.Currencies);
            
            _collection.EvtAdded.AddListener(args =>
            {
                EvtAdded?.Invoke(new WalletCurrencyUpdatedArgs
                {
                    Currency = args.Currency,
                    Previous = args.Previous,
                    Wallet = this
                });
            });
            _collection.EvtDeducted.AddListener(args =>
            {
                EvtDeducted?.Invoke(new WalletCurrencyUpdatedArgs
                {
                    Currency = args.Currency,
                    Previous = args.Previous,
                    Wallet = this
                });
            });
            _collection.EvtUpdated.AddListener(args =>
            {
                _wasUpdated = true;
            });
        }

        private void LateUpdate()
        {
            // Only throw EvtUpdated once every frame if applicable
            if (!_wasUpdated) return;
            _wasUpdated = false;
            EvtUpdated?.Invoke(this);
        }

        #region CurrencyCollection Facade

        public Currency? Get(CurrencyType type) => _collection.Get(type);

        public float GetValue(CurrencyType type) => _collection.GetValue(type);
        
        public Currency Set(Currency currency) => _collection.Set(currency);

        public Currency Set(CurrencyType type, float value) => _collection.Set(type, value);

        public Currency SetMax(CurrencyType type, float max) => _collection.SetMax(type, max);
        
        public Currency Add(Currency currency) => _collection.Add(currency);

        public Currency Add(CurrencyType type, float value) => _collection.Add(type, value);
        
        public IReadOnlyCollection<Currency> Add(params Currency[] currencies) => _collection.Add(currencies);

        public IReadOnlyCollection<Currency> Add(IReadOnlyCollection<Currency> currencies) => _collection.Add(currencies);

        public Currency Deduct(Currency currency) => _collection.Deduct(currency);

        public IReadOnlyCollection<Currency> Deduct(params Currency[] currencies) => _collection.Deduct(currencies);

        public IReadOnlyCollection<Currency> Deduct(IReadOnlyCollection<Currency> currencies) => _collection.Deduct(currencies);

        public bool HasEnough(Currency currency) => _collection.HasEnough(currency);
        
        public bool HasEnough(IReadOnlyCollection<Currency> currencies) => _collection.HasEnough(currencies);

        public bool HasEnough(params Currency[] currencies) => _collection.HasEnough(currencies);

        #endregion
    }
    
    [System.Serializable]
    public struct WalletCurrencyUpdatedArgs
    {
        public Currency Currency { get; set; }
        public Currency Previous { get; set; }
        public Wallet Wallet { get; set; }

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