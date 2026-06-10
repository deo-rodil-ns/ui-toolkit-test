using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Cycles
{
    [CreateAssetMenu(menuName = "Cycle/Behavior", order = 1)]
    public class CycleBehavior : ScriptableObject
    {
        [SerializeField] private CycleData _cycle;
        [SerializeReference, SubclassSelector] private CycleTask[] _tasks;

        public Cycle Cycle { get; private set; }
        public Faction Faction { get; private set; }

        public UnityEvent<CycleBehaviorSnapshot> EvtCycleResolved { get; } = new();

        private bool _initialized;

        public static CycleBehavior Create(CycleBehavior template, Faction faction = null)
        {
            if (template._initialized) throw new Exception("CycleActionSet is not a template");

            var instance = Instantiate(template);
            instance.Initialize(faction);

            return instance;
        }

        private void Initialize(Faction faction)
        {
            _initialized = true;
            Faction = faction;

            // Setup runtime instances
            Cycle = faction.World.GetService<WorldCycleManager>().GetCycle(_cycle) ??
                    throw new Exception("Cycle not found");
            _tasks = _tasks.Select(template =>
            {
                var action = template.Clone();
                action.Initialize(Cycle, faction);
                return action;
            }).ToArray();
            
            // Listen to cycle
            Cycle.EvtCycleCompleted.AddListener(OnCycleCompleted);
        }

        private void OnCycleCompleted(Cycle cycle)
        {
            // Create snapshot
            var snapshot = new CycleBehaviorSnapshot(this);

            // Execute tasks
            _tasks.ForEach(t => t.Execute(snapshot));
            
            EvtCycleResolved?.Invoke(snapshot);
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            
            Cycle?.EvtCycleCompleted.RemoveListener(OnCycleCompleted);
        }
    }
}