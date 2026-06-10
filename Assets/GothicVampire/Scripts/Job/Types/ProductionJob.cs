using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using GothicVampire.Game;
using GothicVampire.Productions;
using GothicVampire.Unrest;
using GothicVampire.Villagers;
using Sylpheed.Extensions;

namespace GothicVampire.Jobs
{
    public sealed class ProductionJob : Job, IUnrestPredictor
    {
        public ProductionOrder Order { get; }
        public bool ShouldLockUntilFirstOrderConclusion { get; set; }
        public UnrestSource Unrest { get; set; }
        
        public IReadOnlyCollection<Currency> InputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> OutputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> NetProjection { get; private set; } = new List<Currency>();
        
        private ProductionManager _productionManager;
        private UnrestActor _unrestActor;

        public ProductionJob(ProductionOrder order)
        {
            // Clone order to make this instance unique
            Order = order.Clone();
            UpdateProjection();
        }

        protected override void OnActivate(IAssignee assignee)
        {
            // Apply modifier
            var villager = assignee as Villager;
            if (villager) Order.OutputModifier = villager.Data.ProductionOutputModifier;
            
            // Lock job until its first conclusion
            if (ShouldLockUntilFirstOrderConclusion)
            {
                Locked = true;
                Order.EvtConcluded.AddListener(OnOrderConcluded);
            }
            
            // Add order
            _productionManager = Faction.GetService<ProductionManager>();
            _productionManager.AddOrder(Order);
            
            // Register unrest predictor
            _unrestActor = Faction.GetService<UnrestActor>();
            if (Unrest != null) _unrestActor.AddPredictor(this);
            
            UpdateProjection();
        }

        protected override void OnDeactivate()
        {
            _productionManager.RemoveOrder(Order);
            
            if (ShouldLockUntilFirstOrderConclusion)
            {
                Locked = false;
                Order.EvtConcluded.RemoveListener(OnOrderConcluded);
            }
            
            if (Unrest != null) _unrestActor.RemovePredictor(this);
            
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            InputProjection = Order.Input
                .Collate()
                .Select(c => c
                    .WithValue(Active ? c.Value : 0)
                    .WithMax(c.Value))
                .ToList();
            
            OutputProjection = Order.Output
                .Collate()
                .Select(c => c
                    .WithValue(Active ? c.Value : 0)
                    .WithMax(c.Value))
                .ToList();
            
            NetProjection = OutputProjection.Separate(InputProjection);
        }
        
        private void OnOrderConcluded(ProductionOrder.ConcludedEvent.Args args)
        {
            if (ShouldLockUntilFirstOrderConclusion) Locked = false;
            
            // Apply unrest
            if (Unrest != null) _unrestActor.AddUnrest(Unrest);
        }

        void IUnrestPredictor.Predict(UnrestSnapshot snapshot)
        {
            // Check if the snapshot cycle will happen on or before this production's cycle
            var cycleManager = World.Current.GetService<WorldCycleManager>();
            var productionCycle = cycleManager.GetCycle(Order.Cycle);
            if (productionCycle.TimeRemaining > snapshot.Cycle.TimeRemaining) return;
            
            snapshot.AddSource(Unrest);
        }
    }
}