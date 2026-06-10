using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Cycles;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Unrest
{
    /// <summary>
    /// Entity where unrest is applied to. A faction is an UnrestActor.
    /// </summary>
    public class UnrestActor : MonoBehaviour, IFactionService
    {
        [SerializeField] private UnrestSettings _settings;
        [SerializeField] private bool _logSource;
        
        [Header("Events")] 
        [SerializeField] private UnityEvent<UnrestSnapshot> _evtResolved;
        [SerializeField] private UnityEvent<UnrestSnapshot> _evtPredicted;
        [SerializeField] private UnityEvent<ValueChangedArgs> _evtValueChanged;
        [SerializeField] private UnityEvent<UnrestSource> _evtSourceAdded;
        [SerializeField] private UnityEvent<UnrestSource> _evtSourceRemoved;
        [SerializeField] private UnityEvent _evtUpdated;
        
        /// <summary>
        /// Value can be changed directly without using UnrestSource.
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                if (Mathf.Approximately(_value, value)) return;
                _value = Mathf.Clamp(value, 0, Max);
                _valueChanged = true;
                _wasUpdated = true;
            }
        }
        public float Max => _settings.MaxValue;
        public float Normalized => Mathf.Clamp01(Value / Max);
        public float ProjectedValue => Value + _persistentSources.Sum(s => s.Value);
        public float ProjectedValueNormalized => Mathf.Clamp01(ProjectedValue / Max);
        public float ProjectedDelta => ProjectedValue - Value;
        
        public IReadOnlyCollection<UnrestSource> PersistentSources => _persistentSources;
        public UnrestSnapshot LastResolved { get; private set; }
        public UnrestSnapshot LastPredicted { get; private set; }
        
        public UnrestSettings Settings => _settings;

        public UnityEvent<UnrestSnapshot> EvtResolved => _evtResolved;
        public UnityEvent<UnrestSnapshot> EvtPredicted => _evtPredicted;
        public UnityEvent<UnrestSource> EvtSourceAdded => _evtSourceAdded;
        public UnityEvent<UnrestSource> EvtSourceRemoved => _evtSourceRemoved;
        public UnityEvent<ValueChangedArgs> EvtValueChanged => _evtValueChanged;
        public UnityEvent EvtUpdated => _evtUpdated;

        public struct ValueChangedArgs
        {
            public UnrestActor Actor;
            public float Value;
            public float PreviousValue;
            public float Delta => Value - PreviousValue;
            public float Normalized => Actor?.Normalized ?? 0f;
        }

        private float _value;
        private readonly List<UnrestSource> _persistentSources = new();
        private readonly List<IUnrestPredictor> _predictors = new();
        private WorldCycleManager _cycleManager;
        private bool _wasUpdated;
        private bool _predictionQueued;
        private bool _valueChanged;
        private float _prevValue;
        
        #region IFactionService

        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _cycleManager = World.Current.GetService<WorldCycleManager>();
            
            // Set default snapshots
            LastResolved = new UnrestSnapshot(_cycleManager.GetCycle(Settings.Cycle), Settings);
            LastResolved.Build();
            LastPredicted = new UnrestSnapshot(_cycleManager.GetCycle(Settings.Cycle), Settings);
            LastPredicted.Build();
            
            EnqueuePrediction();
        }

        #endregion

        /// <summary>
        /// Add an unrest. This will be resolved on the next cycle.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shouldClone">If true, use a cloned source instance. Leave this to true if passed object came from a serialized template. Set to false if constructed from code.</param>
        public UnrestSource AddUnrest(UnrestSource source, bool shouldClone = true)
        {
            // Ignore invalid unrest
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.IsValid) return null;
            
            // Clone source
            if (shouldClone) source = source.Clone();
            
            // Cache persistent sources. Also include sources that are applied on resolve.
            if (source.Persistent || source.ApplyOnResolve) _persistentSources.Add(source);
            
            // Initialize. This can immediately apply the unrest value if configured in source.
            var cycle = _cycleManager.GetCycle(Settings.Cycle) ?? throw new Exception($"Cycle {Settings.Cycle.Id} not found");
            source.Initialize(this, cycle);

            // Log
            if (_logSource) Debug.Log($"[Unrest] {source.Category.Id} added. {Value:N0} ({source.AppliedValue.ToStringWithPrefix("N0")})");
            
            _wasUpdated = true;
            EvtSourceAdded?.Invoke(source);
            
            return source;
        }

        public IReadOnlyCollection<UnrestSource> AddUnrest(IReadOnlyCollection<UnrestSource> sources,
            bool shouldClone = true)
        {
            return sources.Select(s => AddUnrest(s, shouldClone)).ToList();
        }

        public void RemoveUnrest(UnrestSource source)
        {
            var removed = _persistentSources.Remove(source);
            if (!removed) return;

            // Revert applied value if applicable
            if (source.RevertOnRemove)
            {
                Value -= source.AppliedValue;
            }
            
            // Log
            if (_logSource) Debug.Log($"[Unrest] {source.Category.Id} removed. {Value:N0} ({source.AppliedValue.ToStringWithPrefix("N0")})");
            
            _wasUpdated = true;
            EvtSourceRemoved?.Invoke(source);
        }

        public void ClearUnrest()
        {
            if (!_persistentSources.Any()) return;
            _persistentSources.ForEach(RemoveUnrest);
        }

        public void Resolve(CycleBehaviorSnapshot snapshot)
        {
            if (snapshot.Cycle.Data != Settings.Cycle) throw new Exception($"Unrest can only be resolved for {Settings.Cycle.Id} cycle.");
            
            snapshot.Unrest.AddSources(_persistentSources);
            
            // Resolve queued unrest values
            var toResolve = _persistentSources.Where(s => s.ApplyOnResolve).ToList();
            toResolve.ForEach(s => s.Resolve());
            
            snapshot.Unrest.Build();
            LastResolved = snapshot.Unrest;
            snapshot.Unrest.LogToConsole($"[Unrest] Resolved.");

            EvtResolved?.Invoke(snapshot.Unrest);
        }

        public UnrestSnapshot Predict()
        {
            // Create a new snapshot and invoke all predictors
            var snapshot = new UnrestSnapshot(_cycleManager.GetCycle(Settings.Cycle), Settings, Value);
            
            // Include persistent sources that aren't applied on resolve
            snapshot.AddSources(_persistentSources.Where(s => !s.ApplyOnResolve).ToList());
            
            // Invoke predictors
            _predictors.ForEach(predictor => predictor.Predict(snapshot));
            
            // Build snapshot
            snapshot.Build();
            
            LastPredicted = snapshot;
            EvtPredicted?.Invoke(snapshot); 
            _predictionQueued = false;
            
            return snapshot;
        }
        
        public void EnqueuePrediction() => _predictionQueued = true;

        public void AddPredictor(IUnrestPredictor predictor)
        {
            _predictors.Add(predictor);
            EnqueuePrediction();
        }

        public void RemovePredictor(IUnrestPredictor predictor)
        {
            _predictors.Remove(predictor);
            EnqueuePrediction();
        }

        private void LateUpdate()
        {
            if (_wasUpdated)
            {
                EvtUpdated?.Invoke();
                _wasUpdated = false;
            }

            if (_predictionQueued) Predict();

            if (_valueChanged)
            {
                EvtValueChanged?.Invoke(new ValueChangedArgs()
                {
                    Value = _value,
                    PreviousValue = _prevValue,
                    Actor = this
                });
                _valueChanged = false;
                _prevValue = Value;
                
                EnqueuePrediction();
            }
        }
    }
}