using GothicVampire.Game;
using GothicVampire.Villagers;
using UnityEngine;

namespace GothicVampire.Technologies.Prerequisites
{
    /// <summary>
    /// Check for total number of villagers regardless of tier.
    /// </summary>
    [System.Serializable]
    public class TotalPopulationReached : Prerequisite
    {
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
            return "Villagers";
        }

        protected override float OnResolveProgress()
        {
            return _villagerManager.Villagers.Count;
        }
        
        private void OnVillagersUpdated() => Resolve();
    }
}