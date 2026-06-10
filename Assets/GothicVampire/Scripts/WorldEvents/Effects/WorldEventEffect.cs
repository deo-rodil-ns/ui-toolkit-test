using System;
using GothicVampire.Cycles;
using GothicVampire.Game;
using GothicVampire.Unrest;
using NUnit.Framework;
using UnityEngine;

namespace GothicVampire.WorldEvents.Effects
{
    [Serializable]
    public class WorldEventEffect
    {
        [SerializeField] private bool _showBanner;
        [SerializeField] private int _durationInCycles;
        [SerializeField] private UnrestSource _unrestSource;
        
        public int CycleRemaining { get; private set; }
        public int DurationInCycles => _durationInCycles;
        
        public Faction Faction { get; private set; }
        public bool IsActive { get; private set; }
        protected virtual void OnStartEffect() {}
        protected virtual void OnStopEffect() {}
        protected virtual void OnInitialize(WorldEvent worldEvent){}

        private WorldEvent _worldEvent;
        private WorldEventManager _worldEventManager;
        private WorldCycleManager _worldCycleManager;
        private UnrestActor _unrestActor;
        
        private UnrestSource _currentUnrestSource;
        
        public void Initialize(WorldEvent worldEvent)
        {
            OnInitialize(worldEvent);
            
            _worldEvent = worldEvent;
            Faction = worldEvent.Faction;
            _worldEventManager = Faction.GetService<WorldEventManager>() ?? throw new Exception("WorldEventManager not found.");
            _worldCycleManager =  World.Current?.GetService<WorldCycleManager>() ?? throw new Exception("WorldCycleManager not found.");
            _unrestActor = Faction.GetService<UnrestActor>() ?? throw new Exception("UnrestActor not found.");      
            
            IsActive = false;
        }
        
        public void StartEffect()
        {
            if (IsActive) return;
            
            OnStartEffect();
            IsActive = true;

            if (_durationInCycles > 0)
            {
                CycleRemaining = _durationInCycles;
            }
            
            if (_unrestSource.Category)
            {
                _currentUnrestSource = _unrestActor.AddUnrest(_unrestSource);
            }
            
            if (_showBanner)
            {
                _worldEventManager.EvtShowBanner.Invoke(_worldEvent);
            }
            
        }
        
        public void OnCycleComplete(Cycle cycle)
        {
            if (!IsActive) return;

            if (_showBanner)
            {
                _worldEventManager.EvtUpdateBanner.Invoke(_worldEvent);
            }
            
            if (CycleRemaining <= 0)
            {
                StopEffect();
            }
            
            CycleRemaining--;
        }
        
        public void StopEffect()
        {
            OnStopEffect();
            IsActive = false;

            if (_showBanner)
            {
                _worldEventManager.EvtHideBanner.Invoke(_worldEvent);
            }

            if (_unrestSource.Category)
            {
                //NOTE: Add Prediction for UI to read upcoming Famine Event
                _unrestActor.RemoveUnrest(_currentUnrestSource);
            }
            
            _worldEvent.StartTriggerCooldown();
        }
    }
}
