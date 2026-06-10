using System;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Cycles
{
    [Serializable]
    public abstract class CycleTask
    {
        public Cycle Cycle { get; private set; }
        
        protected virtual void OnExecute(CycleBehaviorSnapshot snapshot) { }
        protected virtual void OnInitialize(Cycle cycle, Faction faction) { }
        protected virtual void OnDestroy() { }

        private bool _initialized;
        
        public void Initialize(Cycle cycle, Faction faction = null)
        {
            if (_initialized) return;
            _initialized = true;
            
            Cycle = cycle;
            OnInitialize(cycle, faction);
        }
        
        ~CycleTask()
        {
            if (!_initialized) return;
            
            OnDestroy();
        }

        public void Execute(CycleBehaviorSnapshot snapshot)
        {
            if (!_initialized) return;
            OnExecute(snapshot);
        }
    }
}