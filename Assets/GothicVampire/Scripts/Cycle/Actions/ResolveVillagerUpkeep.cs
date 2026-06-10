using System;
using GothicVampire.Game;
using GothicVampire.Villagers;

namespace GothicVampire.Cycles.Actions
{
    [Serializable]
    public sealed class ResolveVillagerUpkeep : FactionCycleTask
    {
        protected override void OnExecute(Cycle cycle, Faction faction, CycleBehaviorSnapshot snapshot)
        {
            var villagerManager = faction.GetService<VillagerManager>() ?? throw new Exception($"{nameof(VillagerManager)} required");
            villagerManager.ResolveUpkeepCost(snapshot);
        }
    }
}