using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using GothicVampire.Villagers;
using UnityEngine;

namespace GothicVampire.Technologies.Prerequisites
{
    /// <summary>
    /// Check for number of villagers with the specified tier or higher.
    /// </summary>
    [System.Serializable]
    public class VillagerTierReached : Prerequisite
    {
        [SerializeField] private int _tier;
        
        private VillagerManager _villagerManager;

        protected override void OnInitialize(Faction faction)
        {
            _villagerManager = faction.GetService<VillagerManager>();
            
            _villagerManager.EvtUpdated.AddListener(OnVillagersUpdated);
        }
        
        protected override void OnDestroy()
        {
            _villagerManager.EvtUpdated.RemoveListener(OnVillagersUpdated);
        }

        protected override string OnBuildDescription(string template)
        {
            return $"Villager T{_tier} (or higher)";
        }

        protected override float OnResolveProgress()
        {
            return _villagerManager.Villagers.Count(v => v.Tier >= _tier);
        }
        
        private void OnVillagersUpdated() => Resolve();
    }
}