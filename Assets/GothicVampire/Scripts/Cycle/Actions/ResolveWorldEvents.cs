using System;
using GothicVampire.Game;
using GothicVampire.WorldEvents;

namespace GothicVampire.Cycles.Actions
{
    [Serializable]
    public class ResolveWorldEvents : FactionCycleTask
    {
        protected override void OnExecute(Cycle cycle, Faction faction, CycleBehaviorSnapshot snapshot)
        {
            var worldEvents = faction.GetService<WorldEventManager>(); 
            
            worldEvents.WorldEvents.ForEach(x =>
            {
                x.UpdateCurrentCycle(cycle);
            });
        }
    }
}
