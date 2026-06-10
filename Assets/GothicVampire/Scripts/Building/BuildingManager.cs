using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GothicVampire.Technologies;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Buildings
{
    public sealed class BuildingManager : MonoBehaviour, IFactionService
    {
        #region Inspector
        [SerializeField] private BuildingSettings _settings;
        [SerializeField] private Transform _container;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<Building> _evtBuildingAdded;
        [SerializeField] private UnityEvent<Building> _evtBuildingRemoved;
        [SerializeField] private UnityEvent<Building> _evtBuildingConstructed;
        [SerializeField] private UnityEvent<Building> _evtBuildingUpgraded;
        #endregion

        public IReadOnlyCollection<Building> Buildings => _buildings;

        public BuildingSettings Settings => _settings;
        
        public UnityEvent<Building> EvtBuildingAdded => _evtBuildingAdded;
        public UnityEvent<Building> EvtBuildingRemoved => _evtBuildingRemoved;
        public UnityEvent<Building> EvtBuildingConstructed => _evtBuildingConstructed;
        public UnityEvent<Building> EvtBuildingUpgraded => _evtBuildingUpgraded;
        
        private readonly List<Building> _buildings = new();
        private Wallet _wallet;
        private TechnologyManager _technologyManager;
        private List<Technology> _cachedTechnologies;

        #region IFactionService
        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _wallet = faction.GetService<Wallet>();
            _technologyManager = faction.GetService<TechnologyManager>();
            
            // Register prerequisites
            Settings.PurchasableBuildings.SelectMany(b => b.Tiers).ForEach(tier => _technologyManager.RegisterPrerequisiteSource(tier));
        }
        #endregion

        /// <summary>
        /// Check if a building has met all the criteria to purchase it. This doesn't include placement checks.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool CanPurchase(BuildingData data)
        {
            var tier = data.Tiers.FirstOrDefault() ?? throw new Exception($"{data.name} has no tier.");
            
            // Check prerequisites
            if (!_technologyManager.IsUnlocked(tier)) return false;

            // Check build cost
            if (!_wallet.HasEnough(tier.BuildCost)) return false;

            return true;
        }

        /// <summary>
        /// Place and create a building at the given grid position. Call this from the placement system.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="gridPos"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public Building Build(BuildingData data, GridCoord gridPos, Quaternion rotation)
        {
            if (!CanPurchase(data)) return null;
            
            // Instantiate building
            var building = Instantiate(_settings.BuildingPrefab, _container);
            building.gameObject.name = data.DisplayName;
            building.Initialize(data, gridPos, rotation, Faction).Forget();
            _buildings.Add(building);
            
            // Remove building when sold
            building.EvtRemoved.AddListener(b =>
            {
                _buildings.Remove(b);
                EvtBuildingRemoved?.Invoke(b);
            });
            
            // Propagate upgrade event
            building.EvtUpgraded.AddListener(b => EvtBuildingUpgraded?.Invoke(b));

            EvtBuildingAdded?.Invoke(building);
            
            // Wait for construction to be finished
            WaitForBuildingConstruction(building).Forget();
            
            return building;
        }

        private async UniTaskVoid WaitForBuildingConstruction(Building building)
        {
            await UniTask.WaitWhile(() => building.Construction.InProgress);
            EvtBuildingConstructed?.Invoke(building);
        }

        #region Technology
        public BuildingTier GetHighestUnlockedTier(BuildingData data)
        {
            return data.Tiers.LastOrDefault(tier => _technologyManager.IsUnlocked(tier));
        }

        public BuildingTier GetNextLockedTier(BuildingData data, bool includeLastTier = false)
        {
            var tier = data.Tiers.FirstOrDefault(tier => !_technologyManager.IsUnlocked(tier));
            
            // Include last tier if applicable
            if (includeLastTier && tier == null) tier = data.Tiers.LastOrDefault();

            return tier;
        }

        public IReadOnlyCollection<BuildingTier> GetNextLockedTiers(bool includeLastTier = false)
        {
            return Settings.PurchasableBuildings
                .Select(data => GetNextLockedTier(data, includeLastTier))
                .Where(tier => tier != null).ToList();
        }
        
        public IReadOnlyCollection<BuildingTier> GetHighestUnlockedTiers()
        {
            return Settings.PurchasableBuildings.Select(GetHighestUnlockedTier).Where(tier => tier != null).ToList();
        }

        /// <summary>
        /// Find the technology that can unlock this building tier. Prioritizes milestones.
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public Technology GetPrerequisiteTechnology(BuildingTier tier)
        {
            // Prepare list of technologies to check. Milestones are prioritized first. Cache this for performance.
            if (_cachedTechnologies == null) _cachedTechnologies = _technologyManager.Technologies.OrderByDescending(t => _technologyManager.Milestones.Contains(t)).ToList();
            
            // Look for the first technology that can unlock this tier. Only use unlockable for now
            var technology = _cachedTechnologies.FirstOrDefault(t => t.Data.Unlockables.Contains(tier));
            
            return technology;
        }

        /// <summary>
        /// Find the prerequisite group that can unlock this building tier. Prioritizes milestones and technology. If there's no technology, returns the local prerequisite group defined in BuildingTier.
        /// </summary>
        /// <param name="tier"></param>
        /// <returns></returns>
        public PrerequisiteGroup GetPrerequisite(BuildingTier tier)
        {
            // Look for prerequisite technology
            var technology = GetPrerequisiteTechnology(tier);
            if (technology != null) return _technologyManager.GetPrerequisite(technology.Data);
            
            // Use local prerequisite
            return _technologyManager.GetPrerequisite(tier);
        }
        
        #endregion
    }
}