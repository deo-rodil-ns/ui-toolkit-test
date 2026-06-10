using System;
using GothicVampire.Game;
using GothicVampire.Unrest;

namespace GothicVampire.Cycles.Actions
{
    [System.Serializable]
    public sealed class ResolveUnrest : FactionCycleTask
    {
        protected override void OnExecute(Cycle cycle, Faction faction, CycleBehaviorSnapshot snapshot)
        {
            var unrest = faction.GetService<UnrestActor>() ?? throw new Exception($"{nameof(UnrestActor)} required");
            
            if (snapshot.Unrest == null) snapshot.Unrest = new UnrestSnapshot(cycle, unrest.Settings, unrest.Value);
            
            unrest.Resolve(snapshot);
        }
    }
}