using Cysharp.Threading.Tasks;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using GothicVampire.Roads.Effects;
using Sylpheed.Core;
using Sylpheed.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Roads
{
    public enum RoadKind
    {
        DeadEnd,   // 1 direction
        Straight,  // 2 directions opposite (NS or EW)
        Corner,    // 2 directions perpendicular
        IntersectionT, // 3 directions
        IntersectionCross // 4 directions
    }

    public class Road : MonoBehaviour
    {
        #region Inpsector Fields

        [SerializeField] private RoadSettings _settings;
        [SerializeField] private RoadKind _roadKind;

        #endregion

        [Header("Events")]
        [SerializeField] private UnityEvent<Road, IReadOnlyCollection<Currency>> _evtSold;
        [SerializeField] private UnityEvent<Road> _evtUpgraded;
        [SerializeField] private RoadMaterialSystem _roadMaterialSystem;

        public RoadData Data { get; private set; }
        public GridCoord GridPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Faction Faction { get; private set; }
        public GameObject Model { get; private set; }
        public RoadKind RoadKind => _roadKind;

        public int Tier { get; private set; }
        public RoadTier CurrentTier => Data.Tiers[Tier - 1];
        public int MaxTier => Data.Tiers.Count;

        public RoadTier NextTier
        {
            get
            {
                if (!Data.Upgradable) return null;
                if (Tier >= MaxTier) return null;

                return Data.Tiers[Tier - 1];
            }
        }

        public string DisplayName => string.IsNullOrWhiteSpace(CurrentTier.NameOverride) ? Data.DisplayName : CurrentTier.NameOverride;

        public bool CanUpgrade
        {
            get
            {
                if (!Data.Upgradable) return false;
                var nextTier = NextTier;
                if (nextTier == null) return false;
                if (!_wallet.HasEnough(nextTier.BuildCost)) return false;

                return true;
            }
        }

        public RoadEffect Effect { get; private set; }
        public UnityEvent<Road> EvtTierUpdated { get; } = new();
        public UnityEvent<Road> EvtUpgraded => _evtUpgraded;
        public UnityEvent<Road, IReadOnlyCollection<Currency>> EvtSold => _evtSold;

        private Wallet _wallet;
        private IRoadService _roadService;

        private void OnDestroy()
        {

        }

        public async UniTaskVoid Initialize(RoadData data, GridCoord gridPos, Quaternion rotation, Faction faction)
        {
            Data = data;
            GridPosition = gridPos;
            Rotation = rotation;
            Faction = faction;

            _roadMaterialSystem = GetComponent<RoadMaterialSystem>();
            _wallet = faction.GetService<Wallet>();
            _roadService = ServiceLocator.Get<IRoadService>();
            // Sync transform
            UpdatePosition(gridPos, rotation);

            // Initialize pre-construction data/objects
            Tier = 0;
            _roadMaterialSystem.Initialize(Data.Tiers[0]);
            var tier = data.Tiers.FirstOrDefault() ?? throw new Exception("No RoadTier set");

            // Consume resources
            _wallet.Deduct(tier.BuildCost);

            // Activate tier
            ActivateTier(1);
        }

        public void Sell()
        {
            var sellCost = SellValue;

            // Add to wallet
            _wallet.Add(sellCost);

            EvtSold?.Invoke(this, sellCost);
            
            // Remove building
            Destroy(gameObject);
        }

        public void UpdateRoadMaterial()
        {
            if (_roadMaterialSystem == null) return;

            _roadMaterialSystem.UpdateRoadMaterial(GridPosition);
        }

        public void UpdateRoadKind()
        {
            var connections = _roadService.GetRoadConnections(GridPosition, false);

            // Order: [North, East, South, West]
            bool n = connections[0];
            bool e = connections[1];
            bool s = connections[2];
            bool w = connections[3];

            int count = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);

            if (count == 4) _roadKind = RoadKind.IntersectionCross;
            else if (count == 3) _roadKind = RoadKind.IntersectionT;
            else if (count == 2)
            {
                // Straight
                if ((n && s) || (e && w))
                    _roadKind = RoadKind.Straight;
                else
                {
                    // Corner
                    _roadKind = RoadKind.Corner;

                }
            }
            else if (count == 1) _roadKind = RoadKind.DeadEnd;
        }

        public IReadOnlyCollection<Currency> SellValue
        {
            get
            {
                // Compute total cost including upgrades
                List<Currency> totalCost = new();
                foreach (var tier in Data.Tiers.Take(Tier))
                    totalCost = totalCost.Concat(tier.BuildCost).ToList();

                // Apply refund modifier
                totalCost = totalCost.Select(c => c * _settings.SellRate).ToList();

                return totalCost;
            }
        }

        private void ActivateTier(int tierLv)
        {
            if (tierLv > Data.Tiers.Count || tierLv < 1) throw new Exception("Tier index out of range");
            var tier = Data.Tiers[tierLv - 1];
            Tier = tierLv;

            Effect?.Deactivate();

            Effect = tier.Effect?.Clone();
            Effect?.Activate(this);

            EvtTierUpdated?.Invoke(this);
        }

        private void UpdatePosition(GridCoord gridPos, Quaternion rotation)
        {
            GridPosition = gridPos;
            Rotation = rotation;
            transform.localRotation = rotation;

            var grid = _roadService.GridCell;
            var cell = grid.Get(gridPos);
            var center = grid.GetWorldCenter(gridPos);
            transform.position = new Vector3(center.x, 0.01f, center.z);
        }
    }
}
