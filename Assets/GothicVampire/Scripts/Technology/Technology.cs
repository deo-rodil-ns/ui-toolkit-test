using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Technologies
{
    public sealed class Technology
    {
        public TechnologyData Data { get; private set; }
        public Faction Faction { get; private set; }
        
        public bool Unlocked { get; private set; }

        public bool IsMilestone
        {
            get
            {
                var technologyManager = Faction.GetService<TechnologyManager>();
                if (_technologyManager == null) return false;

                return technologyManager.Milestones.Contains(this);
            }
        }

        public bool IsMilestoneActiveOrComplete
        {
            get
            {
                var technologyManager = Faction.GetService<TechnologyManager>();
                if (_technologyManager == null) return false;

                return technologyManager.IsMilestoneActiveOrComplete(Data);
            }
        }

        public PrerequisiteGroup Prerequisites { get; private set; }
        public IReadOnlyCollection<TechnologyEffect> Effects => _effects;

        public UnityEvent<Technology> EvtUnlocked { get; } = new();
        
        private readonly List<TechnologyEffect> _effects;
        private readonly TechnologyManager _technologyManager;
        private readonly Func<Technology, bool> _milestoneChecker;
        
        public Technology(TechnologyData data, Faction faction, Func<Technology, bool> milestoneChecker = null)
        {
            Data = data;
            Faction = faction;
            _technologyManager = Faction.GetService<TechnologyManager>();
            _milestoneChecker = milestoneChecker;
            
            // Initialize prerequisites
            Prerequisites = _technologyManager.RegisterPrerequisiteSource(data);
            Prerequisites.EvtSatisfied?.AddListener(OnPrerequisiteGroupSatisfied);

            // Initialize effects
            _effects = data.Effects.Select(e => e.Clone()).ToList();
            
            _technologyManager.EvtTechnologyUnlocked?.AddListener(OnTechnologyUnlocked);
            _technologyManager.EvtUnlocked?.AddListener(OnUnlockableUnlocked);
        }

        ~Technology()
        {
            _technologyManager?.EvtTechnologyUnlocked?.AddListener(OnTechnologyUnlocked);
            _technologyManager?.EvtUnlocked?.RemoveListener(OnUnlockableUnlocked);
            Prerequisites?.EvtSatisfied?.RemoveListener(OnPrerequisiteGroupSatisfied);
        }
        
        private void Unlock()
        {
            if (Unlocked) return;
            Unlocked = true;
            
            // Unlock unlockables
            Data.Unlockables.ForEach(u => _technologyManager.Unlock(u));
            
            // Activate effects
            _effects.ForEach(e => e.Activate(Faction));
            
            EvtUnlocked?.Invoke(this);
        }

        /// <summary>
        /// Check for all prerequisites. If satisfied, unlock this technology.
        /// </summary>
        private void TryUnlock()
        {
            if (Unlocked) return;

            // Check if prerequisites are satisfied
            if (!Prerequisites.Satisfied) return;
            
            // Check milestones if applicable
            if (!_milestoneChecker?.Invoke(this) ?? false) return;
            
            Unlock();
        }
        
        private void OnTechnologyUnlocked(Technology technology) => TryUnlock();
        private void OnUnlockableUnlocked(IUnlockable unlockable) => TryUnlock();
        private void OnPrerequisiteGroupSatisfied(PrerequisiteGroup prerequisiteGroup) => TryUnlock();
    }
}