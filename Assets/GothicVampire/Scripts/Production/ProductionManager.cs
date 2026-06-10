using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using GothicVampire.Game;
using GothicVampire.Villagers;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Productions
{
    public sealed class ProductionManager : MonoBehaviour, IFactionService
    {
        [SerializeField] private List<CycleData> _cycles;
        [SerializeField] private CycleData _upkeepCycle;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<ProductionBatchReport> _evtBatchCompleted;
        [SerializeField] private UnityEvent<ProductionManager> _evtProjectionUpdated;
        
        private List<ProductionBatch> _batches = new();
        private bool _projectionWasUpdated;

        public IReadOnlyCollection<ProductionBatch> Batches => _batches;

        public IReadOnlyCollection<Currency> InputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> OutputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> NetProjection { get; private set; } = new List<Currency>();

        #region Events

        public UnityEvent<ProductionBatchReport> EvtBatchCompleted => _evtBatchCompleted;
        public UnityEvent<ProductionManager> EvtProjectionUpdated => _evtProjectionUpdated;

        #endregion

        private VillagerManager _villagerManager;
        
        #region IFactionService

        public Faction Faction { get; set; }
        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _villagerManager = faction.GetService<VillagerManager>();
            
            // Create production per batch
            var wallet = faction.GetService<Wallet>();
            _batches = _cycles.Select(b => new ProductionBatch(b, wallet)).ToList();
            
            // Configure and start production
            foreach (var production in _batches)
            {
                production.EvtCompleted.AddListener(r => EvtBatchCompleted?.Invoke(r));

                // Update total projection when one production's projection gets updated
                production.EvtProjectionUpdated.AddListener(_ =>
                {
                    _projectionWasUpdated = true;
                });
            }
        }

        #endregion

        public ProductionBatch GetProduction(CycleData cycle) => _batches.SingleOrDefault(p => p.Cycle.Data == cycle);
        public ProductionBatch GetProduction(Cycle cycle) => _batches.SingleOrDefault(p => p.Cycle == cycle);
        
        public void AddOrder(ProductionOrder order)
        {
            var production = _batches.FirstOrDefault(p => p.Cycle.Data == order.Cycle);
            if (production == null) return;
            
            production.AddOrder(order);
        }

        public void AddOrders(IReadOnlyCollection<ProductionOrder> orders)
        {
            orders.ForEach(AddOrder);
        }

        public void RemoveOrder(ProductionOrder order)
        {
            var production = _batches.FirstOrDefault(p => p.Cycle.Data == order.Cycle);
            if (production == null) return;
            
            production.RemoveOrder(order);
        }

        public void RemoveOrders(IReadOnlyCollection<ProductionOrder> orders)
        {
            orders.ForEach(RemoveOrder);
        }

        private void LateUpdate()
        {
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            if (!_projectionWasUpdated) return;
            
            InputProjection = _batches.SelectMany(p => p.InputProjection).Collate();
            OutputProjection = _batches.SelectMany(p => p.OutputProjection).Collate();
            NetProjection = OutputProjection.Separate(InputProjection);
            
            _projectionWasUpdated = false;
            EvtProjectionUpdated?.Invoke(this);
            
            Debug.Log($"[Production] Projection updated. {NetProjection.FormatToString()}");
        }
    }
}