using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Technologies.Prerequisites
{
    /// <summary>
    /// Check if certain buildings with the specified tiers are built.
    /// </summary>
    [System.Serializable]
    public class BuildingBuilt : Prerequisite
    {
        [SerializeField] private BuildingTier _buildingTier;
        
        private BuildingManager _buildingManager;
        
        protected override void OnInitialize(Faction faction)
        {
            _buildingManager = faction.GetService<BuildingManager>();

            _buildingManager.EvtBuildingConstructed.AddListener(OnBuildingConstructed);
            _buildingManager.EvtBuildingUpgraded.AddListener(OnBuildingUpgraded);
        }
        
        protected override void OnDestroy()
        {
            _buildingManager?.EvtBuildingConstructed.RemoveListener(OnBuildingConstructed);
            _buildingManager?.EvtBuildingUpgraded.RemoveListener(OnBuildingUpgraded);
        }

        protected override string OnBuildDescription(string template)
        {
            return $"{_buildingTier.Building.DisplayName} T{_buildingTier.TierLevel}";
        }

        protected override float OnResolveProgress()
        {
            return _buildingManager.Buildings.Count(b =>
            {
                // Check if same building type
                if (b.Data != _buildingTier.Building) return false;
                
                // Don't count lower tiers
                if (b.CurrentTier.TierLevel < _buildingTier.TierLevel) return false;
                
                // Count building of higher tier regardless of construction state
                if (b.CurrentTier.TierLevel > _buildingTier.TierLevel) return true;
                
                // Same tier, only count if it's not under construction
                return b.Construction.Ready;
            });
        }

        /// <summary>
        /// Only resolve buildings that we're interested in
        /// </summary>
        /// <param name="building"></param>
        private void Evaluate(Building building)
        {
            if (building.Data != _buildingTier.Building) return;
            Resolve();
        }

        private void OnBuildingConstructed(Building building) => Evaluate(building);
        private void OnBuildingUpgraded(Building building) => Evaluate(building);
    }
}