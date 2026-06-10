using System.Collections.Generic;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Productions;
using GothicVampire.Unrest;

namespace GothicVampire.Cycles
{
    public sealed class CycleBehaviorSnapshot
    {
        public CycleBehavior Behavior { get; }
        public Cycle Cycle => Behavior.Cycle;
        public Faction Faction => Behavior.Faction;
        
        public CurrencyCollection CurrencyChanged { get; } = new();
        public ICollection<ProductionBatchReport> ProductionBatchReports { get; } = new List<ProductionBatchReport>();
        public UnrestSnapshot Unrest { get; set; }

        public CycleBehaviorSnapshot(CycleBehavior behavior)
        {
            Behavior = behavior;
        }
    }
}