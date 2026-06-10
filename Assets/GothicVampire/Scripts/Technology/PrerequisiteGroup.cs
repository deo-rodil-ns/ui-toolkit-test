using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using Sylpheed.Extensions;
using TNRD;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Technologies
{
    [System.Serializable]
    public sealed class PrerequisiteGroup
    {
        [Header("Source")]
        [SerializeField] private bool _unlockSourceIfSatisfied;
        
        [Header ("Prerequisites")]
        [SerializeField] private TechnologyData[] _technologies;
        [SerializeField] private SerializableInterface<IUnlockable>[] _unlockables;
        [SerializeReference, SubclassSelector] private Prerequisite[] _customs;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<PrerequisiteGroup> _evtSatisfied;
        [SerializeField] private UnityEvent<PrerequisiteGroup> _evtProgressUpdated;
        
        public bool Initialized { get; private set; }
        public Faction Faction { get; private set; }
        public IPrerequisiteSource Source { get; private set; }
        public IUnlockable SourceUnlockable => Source?.Unlockable;

        public bool Satisfied
        {
            get
            {
                if (!Initialized) throw new Exception("Not yet initialized");
                
                // Always satisfied if there's nothing to check
                if (!Any()) return true;
                
                // Check unlockables
                if (!Unlockables.All(Faction.IsUnlocked)) return false;
            
                // Check technologies
                if (!_technologies.All(Faction.IsUnlocked)) return false;
                
                // Check prerequisites
                if (!_customs.All(p => p.Satisfied)) return false;
                
                return true;
            }
        }
        
        public float Progress
        {
            get
            {
                var numFactors = Unlockables.Count + Technologies.Count + Customs.Count;
                if (numFactors == 0) return 0f;
                
                var unlockableFactor = Unlockables.Count(u => Faction.IsUnlocked(u));
                var technologyFactor = Technologies.Count(t => Faction.IsUnlocked(t));
                var prerequisiteFactor = Customs.Sum(p => p.ProgressRate);
                var total = unlockableFactor + technologyFactor + prerequisiteFactor;
                
                return Mathf.Clamp01(total / numFactors);
            }
        }
        
        public IReadOnlyCollection<TechnologyData> Technologies => _technologies;
        public IReadOnlyCollection<IUnlockable> Unlockables => Initialized ? _cachedUnlockables : _unlockables.Select(u => u.Value).ToArray();
        public IReadOnlyCollection<Prerequisite> Customs => _customs;
        
        public UnityEvent<PrerequisiteGroup> EvtSatisfied => _evtSatisfied;
        public UnityEvent<PrerequisiteGroup> EvtProgressUpdated => _evtProgressUpdated;
        
        private bool _prevSatisfied;
        private TechnologyManager _technologyManager;
        private List<IUnlockable> _cachedUnlockables = new();

        /// <summary>
        /// Initialize and bind this prerequisite to a faction. This should only be called internally by TechnologyManager. Use TechnologyManager.RegisterPrerequisiteSource instead.
        /// </summary>
        /// <param name="faction"></param>
        /// <param name="source"></param>
        /// <exception cref="Exception"></exception>
        public void Initialize(Faction faction, IPrerequisiteSource source = null)
        {
            if (Initialized) throw new Exception("Already initialized");
            Initialized = true;
            Faction = faction;
            Source = source;
            _technologyManager = faction.GetService<TechnologyManager>();

            // Cache unlockables so that the list is only built once
            _cachedUnlockables = _unlockables.Select(u => u.Value).ToList();
            
            // Initialize prerequisites
            Customs.ForEach(p => p.Initialize(Faction, false));
            
            // Setup events
            _technologyManager.EvtUnlocked?.AddListener(OnUnlockableUnlocked);
            _technologyManager.EvtTechnologyUnlocked?.AddListener(OnTechnologyUnlocked);
            Customs.ForEach(p =>
            {
                p.EvtProgressUpdated?.AddListener(OnPrerequisiteProgressUpdated);
            });
            
            // Manually resolve prerequisites so that the result is ready on the same frame.
            Customs.ForEach(p => p.Resolve());
        }

        ~PrerequisiteGroup()
        {
            if (!Initialized) return;
            
            _technologyManager?.EvtUnlocked?.RemoveListener(OnUnlockableUnlocked);
            _technologyManager?.EvtTechnologyUnlocked?.RemoveListener(OnTechnologyUnlocked);
            Customs.ForEach(p =>
            {
                p?.EvtProgressUpdated?.RemoveListener(OnPrerequisiteProgressUpdated);
            });
        }
        
        public bool Any()
        {
            return Technologies.Any() || Unlockables.Any() || Customs.Any();
        }

        private void EvaluateStateChange()
        {
            EvtProgressUpdated?.Invoke(this);
            
            // Only throw event when state changed from not satisfied to satisfied
            var current = Satisfied;
            if (_prevSatisfied == current) return;
            _prevSatisfied = current;
            if (!current) return;
            
            if (_unlockSourceIfSatisfied && SourceUnlockable != null) _technologyManager.Unlock(SourceUnlockable);
            EvtSatisfied?.Invoke(this);
        }

        private void OnUnlockableUnlocked(IUnlockable unlockable)
        {
            if (Unlockables.All(u => u != unlockable)) return;
            EvaluateStateChange();
        }

        private void OnTechnologyUnlocked(Technology technology)
        {
            if (Technologies.All(t => t != technology.Data)) return;
            EvaluateStateChange();
        }

        private void OnPrerequisiteProgressUpdated(Prerequisite prerequisite) => EvaluateStateChange();
    }

    public interface IPrerequisiteSource
    {
        PrerequisiteGroup Prerequisites { get; }

        /// <summary>
        /// Source is tied to an unlockable. By default, it'll try to check if this same instance is also an Unlockable. Else, set manually.
        /// </summary>
        IUnlockable Unlockable => this as IUnlockable;
    }
}