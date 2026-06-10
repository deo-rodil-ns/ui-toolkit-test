using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Cycles;
using GothicVampire.Game;
using GothicVampire.Productions;
using GothicVampire.WorldEvents.Effects;
using GothicVampire.WorldEvents.Triggers;
using Sylpheed.Core;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    public class WorldEvent
    {
        public WorldEventData Data { get; }
        
        public string Category => Data.Category;
        public string Description => Data.Description;
        public string DisplayName => Data.DisplayName;
        
        public bool IsActive { get; private set; }
        public int CycleLastTriggered { get; private set; }
        public int CurrentCycleCount { get; private set; }
        public IReadOnlyCollection<WorldEventTrigger> Triggers { get; private set; }
        public IReadOnlyCollection<WorldEventEffect> Effects  { get; private set; }
        public IReadOnlyCollection<WorldEventResult> Results => _results;
        
        public Faction Faction { get; private set; }
        public Cycle Cycle { get; private set; }
        
        protected virtual void OnUpdate(float dt) { }
        
        private ProductionManager _productionManager;
        private WorldCycleManager _worldCycleManager;
        
        private List<WorldEventResult> _results = new List<WorldEventResult>();
        
        public WorldEvent(WorldEventData data, Faction faction)
        {
            Data = data;
            Faction = faction;
            _productionManager = faction.GetService<ProductionManager>() ?? throw new Exception("ProductionManager not found.");
            _worldCycleManager = World.Current.GetService<WorldCycleManager>() ?? throw new Exception("WorldCycleManager not found.");
            
            Triggers = data.Triggers.Select(x =>
            {
                var trigger = x.Clone();
                trigger.Initialize(this);
                return trigger;
            }).ToList();

            Effects = data.Effect.Select(x =>
            {
                var effect = x.Clone();
                effect.Initialize(this);
                return effect;
            }).ToList();
            
            IsActive = true;
        }

        public void Update(float dt)
        {
            if (!IsActive) return;
            
            UpdateWorldEventProgress();
            OnUpdate(dt);
        }

        public void UpdateCurrentCycle(Cycle cycle)
        {
            Cycle = cycle;
            CurrentCycleCount = cycle.NumCyclesCompleted + 1;
            UpdateWorldEventProgress();

            Effects.ForEach(x => { x.OnCycleComplete(cycle); });
        }

        public void StartTriggerCooldown()
        {
            if (!AllEffectsCompleted()) return;
            
            Triggers.ForEach(x => x.StartCooldown());
        }
        
        private void UpdateWorldEventProgress()
        {
            // If an effect from the world event is still active, it cannot be triggered.
            if (AllEffectsCompleted())
            {
                Triggers.ForEach(x =>
                {
                    if (!x.OnCooldown)
                    {
                        x.Resolve();
                    }
                });   
            }

            if (Triggers.All(x => x.IsTriggered) && Effects.All(x => !x.IsActive))
            {
                StartEffects();
            }
        }
        
        private void StartEffects()
        {
            Effects.ForEach(x => x.StartEffect());
            
            CycleLastTriggered = _worldCycleManager.MainCycle.NumCyclesCompleted;
            
            //TODO: Create WorldEventResult to feed on WorldEventManager
            _results.Add(new WorldEventResult(this, _worldCycleManager.MainCycle.NumCyclesCompleted));
        }

        private bool AllEffectsCompleted()
        {
            return Effects.All(x => !x.IsActive);
        }
    }
}
