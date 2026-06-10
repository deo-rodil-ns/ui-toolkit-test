using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Technologies
{
    public sealed class TechnologyManager : MonoBehaviour, IFactionService, IUnlockableResolver
    {
        [SerializeField] private TechnologySettings _settings;
        [SerializeField] private TechnologySet _milestoneSet;
        
        [SerializeField] private UnityEvent<IUnlockable> _evtUnlocked;
        [SerializeField] private UnityEvent<Technology> _evtTechnologyUnlocked;
        [SerializeField] private UnityEvent<Technology> _evtMilestoneCompleted;
        [SerializeField] private UnityEvent _evtUpdated;
        
        public IReadOnlyCollection<Technology> Technologies => _technologies;
        public IReadOnlyCollection<IUnlockable> Unlockables => _unlockables;
        public IReadOnlyCollection<IUnlockable> UnlockedUnlockables => _unlockedUnlockables;
        
        public IReadOnlyList<Technology> Milestones { get; private set; } = new List<Technology>();
        public Technology ActiveMilestone => Milestones.FirstOrDefault(t => !t.Unlocked);
        
        public UnityEvent<IUnlockable> EvtUnlocked => _evtUnlocked;
        public UnityEvent<Technology> EvtTechnologyUnlocked => _evtTechnologyUnlocked;
        public UnityEvent<Technology> EvtMilestoneCompleted => _evtMilestoneCompleted;
        public UnityEvent EvtUpdated => _evtUpdated;

        private readonly List<Technology> _technologies = new();
        private readonly HashSet<IUnlockable> _unlockedUnlockables = new();
        private readonly HashSet<IUnlockable> _unlockables = new();
        private readonly Dictionary<IPrerequisiteSource, PrerequisiteGroup> _prerequisiteMap = new();
        private bool _wasUpdated;

        public void Unlock(IUnlockable unlockable)
        {
            // Add unlockable if it doesn't exist yet
            if (!_unlockedUnlockables.Add(unlockable)) return;
            
            Debug.Log($"[Technology] {unlockable.Name} unlocked.");
            EvtUnlocked?.Invoke(unlockable);
            _wasUpdated = true;
        }
        
        public void Unlock(IReadOnlyCollection<IUnlockable> unlockables) => unlockables.ForEach(Unlock);
        public void UnlockAll() => _settings.Unlockables.ForEach(Unlock);

        public bool IsUnlocked(TechnologyData technology)
        {
            return _technologies.SingleOrDefault(t => t.Data == technology)?.Unlocked ?? false;
        }
        
        public bool IsUnlocked(IUnlockable unlockable)
        {
            return _unlockedUnlockables.Contains(unlockable);
        }

        private Technology AddTechnology(TechnologyData data)
        {
            // Add a milestone checker if it's part of the milestone set
            Func<Technology, bool> milestoneChecker = _milestoneSet.Technologies.Any(d => d == data) 
                ? IsMilestoneActiveOrComplete 
                : null;
            
            var technology = new Technology(data, Faction, milestoneChecker);
            technology.EvtUnlocked?.AddListener(t =>
            {
                _evtTechnologyUnlocked?.Invoke(t);
                
                // Send a separate event for milestone completion
                if (Milestones.Contains(t)) EvtMilestoneCompleted?.Invoke(t);
                
                _wasUpdated = true;
                Debug.Log($"[Technology] {t.Data.name} unlocked.");
            });
            
            _technologies.Add(technology);
                
            return technology;
        }

        #region PrerequisiteGroup Cache
        public PrerequisiteGroup GetPrerequisite(IPrerequisiteSource source) => _prerequisiteMap.GetValueOrDefault(source);

        public PrerequisiteGroup RegisterPrerequisiteSource(IPrerequisiteSource source)
        {
            if (!_prerequisiteMap.TryGetValue(source, out var prerequisiteGroup))
            {
                // If prerequisite group is not yet initialized, clone and initialize it.
                prerequisiteGroup = source.Prerequisites.Initialized
                    ? source.Prerequisites
                    : source.Prerequisites.Clone();
                if (!prerequisiteGroup.Initialized) prerequisiteGroup.Initialize(Faction, source);
                
                // Cache prerequisite
                _prerequisiteMap.Add(source, prerequisiteGroup);
            }
            
            return prerequisiteGroup;
        }

        public void RemovePrerequisiteSource(IPrerequisiteSource source) => _prerequisiteMap.Remove(source);
        
        public bool IsPrerequisiteSatisfied(IPrerequisiteSource source) => GetPrerequisite(source)?.Satisfied ?? false;
        #endregion
        

        private bool IsMilestoneActiveOrComplete(Technology technology)
        {
            return IsMilestoneActiveOrComplete(technology.Data);
        }
        
        public bool IsMilestoneActiveOrComplete(TechnologyData data)
        {
            // Get technology
            var technology = Milestones.SingleOrDefault(m => m.Data == data);
            if (technology == null) return false;
            
            // Check unlocked
            if (technology.Unlocked) return true;
            
            // Cannot complete milestones that aren't active yet
            if (ActiveMilestone == null) return false;
            if (ActiveMilestone != technology) return false;
            
            return true;
        }

        public TechnologyData GetTierMilestoneUnlocked(BuildingTier tier)
        {
            TechnologyData data = _milestoneSet.Technologies.FirstOrDefault(x => x.Unlockables.Contains(tier));
            return data;
        }

        private void Update()
        {
            foreach (var technology in _technologies)
            {
                technology.Effects.ForEach(e => e.Update(Time.deltaTime));
            }
        }

        private void LateUpdate()
        {
            if (!_wasUpdated) return;
            _wasUpdated = false;
            EvtUpdated?.Invoke();
        }

        #region IFactionService

        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _technologies.Clear();
            
            // Create technology milestones from data
            Milestones = _milestoneSet.Technologies.Select(AddTechnology).ToList();
            
            // Create technologies from data. Do not duplicate milestone entries
            _settings.Technologies
                .Where(d => Milestones.All(m => m.Data != d))
                .ForEach(d => AddTechnology(d));

            // Add unlockables
            _settings.Unlockables.ForEach(u => _unlockables.Add(u));
            _settings.UnlockablesToPreUnlock.ForEach(Unlock);
        }

        #endregion
    }

    public static class FactionTechnologyManagerExtension
    {
        public static IUnlockableResolver GetUnlockableResolver(this Faction faction) => faction.GetService<TechnologyManager>() ?? throw new Exception("Cannot get unlockable resolver for faction " + faction.name);
        public static bool IsUnlocked(this Faction faction, IUnlockable unlockable) => GetUnlockableResolver(faction)?.IsUnlocked(unlockable) ?? false;
        public static bool IsUnlocked(this Faction faction, TechnologyData technology) => faction.GetService<TechnologyManager>()?.IsUnlocked(technology) ?? false;
        public static void Unlock(this Faction faction, IUnlockable unlockable) => faction.GetService<TechnologyManager>()?.Unlock(unlockable);
        public static void Unlock(this Faction faction, IReadOnlyCollection<IUnlockable> unlockables) => faction.GetService<TechnologyManager>()?.Unlock(unlockables);
    }
}