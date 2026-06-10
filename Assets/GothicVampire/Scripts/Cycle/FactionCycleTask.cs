using System;
using GothicVampire.Game;

namespace GothicVampire.Cycles
{
    public abstract class FactionCycleTask : CycleTask
    {
        public Faction Faction { get; private set; }
        
        protected virtual void OnInitialize(Faction faction) { } 
        protected virtual void OnDestroy(Faction faction) { }
        protected virtual void OnExecute(Cycle cycle, Faction faction, CycleBehaviorSnapshot snapshot) { }

        protected sealed override void OnInitialize(Cycle cycle, Faction faction)
        {
            Faction = faction ?? throw new Exception("Faction is required.");
            OnInitialize(faction);
        }

        protected sealed override void OnExecute(CycleBehaviorSnapshot snapshot)
        {
            OnExecute(snapshot.Cycle, Faction, snapshot);
        }

        protected sealed override void OnDestroy()
        {
            OnDestroy(Faction);
        }
    }
}