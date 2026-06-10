using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.WorldEvents.Triggers
{
    [Serializable]
    public class WorldEventTrigger
    {
        [Header("Prerequisites")] 
        [SerializeField] private List<CurrencyThreshold> _currencies;

        [Header("Cycle Prerequisites")]

        [SerializeField] private bool _enableCycleTrigger;
        [SerializeField] private bool _triggeredOnInterval;
        [SerializeField] private int _cycleTriggered;
        [SerializeField] private float _cycleCooldown;
        
        private int _activateOnCycle;
        private Wallet _wallet;

        private float _cycleCooldownCounter;
        public bool OnCooldown { get; protected set; }
        public bool IsTriggered { get; protected set; }
        protected virtual void OnResolve() { }
        protected virtual void OnInitialize() { }
        protected Faction Faction { get; private set; }

        private WorldEvent _worldEvent;

        public void Initialize(WorldEvent worldEvent)
        {
            _worldEvent = worldEvent;
            Faction = _worldEvent.Faction;
            _wallet = Faction.GetService<Wallet>();
            _activateOnCycle = _cycleTriggered;
            OnInitialize();
        }
        
        public void Resolve()
        {
            var currenciesTriggered = _currencies.Count == 0 || _currencies.All(x =>
            {
                var currency = _wallet.GetValue(x.CurrencyType);
                return x.Threshold >= currency;
            });
            
            var cycleCompleted =  _worldEvent.CurrentCycleCount >= _activateOnCycle;

            IsTriggered = currenciesTriggered && cycleCompleted;
            
            OnResolve();
        }

        public void StartCooldown()
        {
            IsTriggered = false;
            OnCooldown = true;
            _worldEvent.Cycle.EvtCycleCompleted.AddListener(Cooldown);
        }

        public void Cooldown(Cycle cycle)
        {
            if (!OnCooldown) return;
            
            _cycleCooldownCounter++;
            
            if (_cycleCooldownCounter >= _cycleCooldown)
            {
                _cycleCooldownCounter = 0;
                OnCooldown = false;
                _worldEvent.Cycle.EvtCycleCompleted.RemoveListener(Cooldown);
            }
        }
    }
    
    [Serializable]
    public class CurrencyThreshold
    {
        [SerializeField] private CurrencyType _currencyType;
        [SerializeField] private int _threshold;
        
        public CurrencyType CurrencyType => _currencyType;
        public float Threshold => _threshold;
    }
}
