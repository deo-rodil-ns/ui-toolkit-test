using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using GothicVampire.Player.Inputs.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Roads
{
    public class RoadManager : MonoBehaviour, IFactionService
    {
        #region Inspector
        [SerializeField] private RoadSettings _roadSettings;
        [SerializeField] private Transform _container;

        [Header("Events")]
        [SerializeField] private UnityEvent<Road> _evtRoadAdded;
        [SerializeField] private UnityEvent<Road> _evtRoadRemoved;
        #endregion

        public RoadSettings Settings => _roadSettings;
        public UnityEvent<Road> EvtRoadAdded => _evtRoadAdded;
        public UnityEvent<Road> EvtRoadRemoved => _evtRoadRemoved;
        
        private List<Road> _roads = new List<Road>();
        private Wallet _wallet;

        #region IFactionService
        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _wallet = faction.GetService<Wallet>();
        }
        #endregion

        public bool CanPurchase(RoadData data)
        {
            return _wallet.HasEnough(data.Tiers.FirstOrDefault()?.BuildCost);
        }

        public Road Build(RoadData data, GridCoord gridPos, Quaternion rotation)
        {
            if (!CanPurchase(data)) return null;

            // Instantiate building
            var road = Instantiate(_roadSettings.RoadPrefab, _container);
            road.gameObject.name = data.DisplayName;
            road.Initialize(data, gridPos, rotation, Faction).Forget();
            _roads.Add(road);

            // Remove building when sold
            road.EvtSold.AddListener((b, cost) =>
            {
                _roads.Remove(road);
                EvtRoadRemoved?.Invoke(road);
            });

            EvtRoadAdded?.Invoke(road);

            return road;
        }

        public void UpdateRoads()
        {
            foreach (Road road in _roads)
            {
                road.UpdateRoadKind();
                road.UpdateRoadMaterial();
            }
        }
    }
}
