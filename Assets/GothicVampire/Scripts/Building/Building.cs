using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using GothicVampire.Player.Inputs.Entity;
using GothicVampire.Technologies;
using GothicVampire.Villagers;
using Sylpheed.Core;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Buildings
{
    public sealed class Building : MonoBehaviour
    {
        [SerializeField] private BuildingSettings _settings;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<Building, IReadOnlyCollection<Currency>> _evtSold;
        [SerializeField] private UnityEvent<Building> _evtRelocated;
        [SerializeField] private UnityEvent<Building> _evtRemoved;
        
        [Header("Events - Upgrade")]
        [SerializeField] private UnityEvent<Building> _evtUpgrading;
        [SerializeField] private UnityEvent<Building> _evtUpgraded;
        [SerializeField] private UnityEvent<Building> _evtUpgradeCancelling;
        [SerializeField] private UnityEvent<Building> _evtUpgradeCancelled;
        
        public BuildingData Data { get; private set; }
        public GridCoord GridPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Faction Faction { get; private set; }
        public VillagerSource VillagerSource { get; private set; }

        public GameObject Model
        {
            get => _model;
            set
            {
                _model = value;
                
                // Find door
                Doors = _model?.GetComponentInChildren<ModelVisualInputEffects>()?.Doors;
            }
        }
        public List<Transform> Doors { get; private set; }
        
        public int MaxTier => Data.Tiers.Max(t => t.TierLevel);
        public BuildingTier CurrentTier { get; private set; }
        public BuildingTier NextTier
        {
            get
            {
                if (!Data.Upgradable) return null;
                if (!CurrentTier) return null;
                if (CurrentTier?.TierLevel >= MaxTier) return null;

                var currentIndex = Data.Tiers.IndexOf(CurrentTier);
                return Data.Tiers.Skip(currentIndex + 1).FirstOrDefault();
            }
        }
        
        public string DisplayName => CurrentTier?.DisplayName ?? Data.DisplayName;

        public bool CanUpgrade
        {
            get
            {
                if (!Data.Upgradable) return false;
                var nextTier = NextTier;
                if (nextTier == null) return false;
                if (Construction.InProgress) return false;
                if (!_wallet.HasEnough(nextTier.BuildCost)) return false;
                if (!_technologyManager.IsPrerequisiteSatisfied(nextTier)) return false;
                
                return true;
            }
        }

        public IReadOnlyCollection<Currency> SellValue
        {
            get
            {
                // Compute total cost including upgrades
                List<Currency> totalCost = new();
                var numTiers = Data.Tiers.IndexOf(CurrentTier) + 1;
                foreach (var tier in Data.Tiers.Take(numTiers))
                    totalCost = totalCost.Concat(tier.BuildCost).ToList();
            
                // Apply refund modifier
                totalCost = totalCost.Select(c => c * _settings.SellRate).ToList();
                
                return totalCost;
            }
        }
        
        /// <summary>
        /// Currency to be refunded when interrupting an ongoing construction. Empty, when there's no construction.
        /// </summary>
        public IReadOnlyCollection<Currency> PendingCost { get; private set; } = new List<Currency>();
        
        public BuildingEffect Effect { get; private set; }
        public BuildingConstruction Construction { get; private set; }
        public BuildingJobAssignment JobAssignment { get; private set; }
        
        public UnityEvent<Building, IReadOnlyCollection<Currency>> EvtSold => _evtSold;
        public UnityEvent<Building> EvtRelocated => _evtRelocated;
        public UnityEvent<Building> EvtUpgrading => _evtUpgrading;
        public UnityEvent<Building> EvtUpgraded => _evtUpgraded;
        public UnityEvent<Building> EvtUpgradeCancelling => _evtUpgradeCancelling;
        public UnityEvent<Building> EvtUpgradeCancelled => _evtUpgradeCancelled;
        public UnityEvent<Building> EvtTierUpdated { get; } = new();
        public UnityEvent<Building> EvtRemoved => _evtRemoved;
        
        private Wallet _wallet;
        private IBuildingService _gridService;
        private TechnologyManager _technologyManager;
        private GameObject _model;
        private bool _initialized;

        private void Awake()
        {
            Construction = GetComponent<BuildingConstruction>();
        }

        public async UniTaskVoid Initialize(BuildingData data, GridCoord gridPos, Quaternion rotation, Faction faction)
        {
            _initialized = true;
            Data = data;
            GridPosition = gridPos;
            Rotation = rotation;
            Faction = faction;
            
            _wallet = faction.GetService<Wallet>();
            _technologyManager = faction.GetService<TechnologyManager>();
            _gridService = ServiceLocator.Get<IBuildingService>();

            // Sync transform
            UpdatePosition(gridPos, rotation);
            
            // Initialize pre-construction data/objects
            CurrentTier = data.Tiers.FirstOrDefault() ?? throw new Exception("No BuildingTier set");
            if (data.CanAssignJobs) JobAssignment = new BuildingJobAssignment(this);
            
            // Consume resources
            _wallet.Deduct(CurrentTier.BuildCost);
            PendingCost = CurrentTier.BuildCost;
            
            // Wait for construction
            var success = await Construction.Execute(this, CurrentTier);
            if (!success)
            {
                // Refund and destroy
                _wallet.Add(PendingCost);
                PendingCost = new List<Currency>();
                EvtRemoved?.Invoke(this);
                Destroy(gameObject);
                return;
            }
            PendingCost = new List<Currency>();
            
            // Setup villager source if enabled
            if (Data.CanHouseVillagers) VillagerSource = new VillagerSource(faction, this);

            // Activate tier
            ActivateTier(CurrentTier);
        }

        public void Sell()
        {
            var sellCost = SellValue;
            
            // Add to wallet
            _wallet.Add(sellCost);
            
            EvtSold?.Invoke(this, sellCost);
            
            // Remove building
            EvtRemoved?.Invoke(this);
            Destroy(gameObject);
        }

        public void Relocate(GridCoord gridPos, Quaternion rotation)
        {
            UpdatePosition(gridPos, rotation);
            EvtRelocated?.Invoke(this);
        }

        public async UniTaskVoid Upgrade()
        {
            if (!CanUpgrade) return;
            
            EvtUpgrading?.Invoke(this);

            // Cache next tier
            var nextTier = NextTier;

            // Consume resources for next tier
            _wallet.Deduct(nextTier.BuildCost);
            PendingCost = nextTier.BuildCost;
            
            // Deactivate effect during upgrade if it's configured
            if (Data.ShouldDeactivateEffectOnUpgrade) Effect?.Deactivate();
            
            // Wait for construction. Reset to current tier if it failed.
            var success = await Construction.Execute(this, nextTier);
            if (!success)
            {
                // Refund cost and reset state
                EvtUpgradeCancelled?.Invoke(this);
                _wallet.Add(PendingCost);
                PendingCost = new List<Currency>();
                ResetToCurrentTier();
                EvtUpgradeCancelled?.Invoke(this);
                return;
            }
            PendingCost = new List<Currency>();
            
            // Update tier
            ActivateTier(nextTier);

            EvtUpgraded?.Invoke(this);
        }

        private void Update()
        {
            if (!_initialized) return;
            
            Effect?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Effect?.Deactivate();
            JobAssignment?.UnassignAll();
        }

        private void ActivateTier(BuildingTier tier)
        {
            CurrentTier = tier;
            
            // Deactivate previous effect if possible
            Effect?.Deactivate();
            
            // Active new effect
            Effect = tier.Effect?.Clone();
            Effect?.Activate(this);
            
            // Unlock unlockables
            Faction.Unlock(tier.UnlockablesOnBuild);
            
            EvtTierUpdated?.Invoke(this);
        }

        private void ResetToCurrentTier()
        {
            Model = Instantiate(CurrentTier.Model, transform);
            ActivateTier(CurrentTier);
        }

        private void SyncPosition() => UpdatePosition(GridPosition, Rotation);
        
        private void UpdatePosition(GridCoord gridPos, Quaternion rotation)
        {
            GridPosition = gridPos;
            Rotation = rotation;
            
            // Translate grid to world pos
            var gridToWorldPos = _gridService.GridToWorldPosition(gridPos);

            transform.localPosition = gridToWorldPos;
            transform.localRotation = rotation;
        }
    }
}