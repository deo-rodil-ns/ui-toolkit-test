using System;
using System.Linq;
using GothicVampire.Game;
using GothicVampire.Productions;
using UnityEngine;

namespace GothicVampire.Cycles.Actions
{
    [Serializable]
    public sealed class ResolveProductionBatch : FactionCycleTask
    {
        protected override void OnExecute(Cycle cycle, Faction faction, CycleBehaviorSnapshot snapshot)
        {
            var productionManager = faction.GetService<ProductionManager>() ?? throw new Exception($"{nameof(ProductionManager)} required");
            var batch = productionManager.GetProduction(cycle.Data) ?? throw new Exception($"{nameof(ProductionBatch)} {cycle.Data.Id} not found");
            
            batch.ResolveOrders(snapshot);
        }
    }
}