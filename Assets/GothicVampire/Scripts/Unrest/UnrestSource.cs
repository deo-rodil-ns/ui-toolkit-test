using System;
using GothicVampire.Cycles;
using UnityEngine;

namespace GothicVampire.Unrest
{
    [System.Serializable]
    public class UnrestSource
    {
        [SerializeField] private UnrestCategory _category;
        [SerializeField] private float _value;
        [SerializeField] private bool _applyImmediately;
        [SerializeField] private bool _applyOnResolve;
        [SerializeField] private bool _revertOnRemove;
        [SerializeField] private bool _removeOnResolve;
        [SerializeField] private bool _persistent;
        
        public float Value => _value;
        public UnrestCategory Category => _category;
        public bool ApplyImmediately => _applyImmediately;
        public bool ApplyOnResolve => _applyOnResolve;
        public bool RevertOnRemove => _revertOnRemove;
        public bool RemoveOnResolve => _removeOnResolve;
        public bool Persistent => _persistent;
        
        public Cycle Cycle { get; private set; }
        public UnrestActor Target { get; private set; }
        public bool Initialized { get; private set; }
        
        public float AppliedValue => AppliedImmediateValue + AppliedResolvedValue;
        public float AppliedImmediateValue { get; private set; }
        public float AppliedResolvedValue { get; private set; }

        public bool IsValid
        {
            get
            {
                if (Mathf.Approximately(Value, 0)) return false;
                if (!_category) return false;
                return true;
            }
        }
        
        // Can only be created from the inspector.
        private UnrestSource() { }

        public void Initialize(UnrestActor target, Cycle cycle = null)
        {
            if (Initialized) throw new Exception("Already initialized");
            Initialized = true;
            
            Target = target;
            Cycle = cycle;

            // Immediately apply value if applicable
            if (ApplyImmediately)
            {
                var prevValue = Target.Value;
                Target.Value += Value;
                AppliedImmediateValue = Mathf.Max(0, Target.Value - prevValue);
            }
        }
        
        /// <summary>
        /// Removes this source from the target UnrestActor
        /// </summary>
        public void Remove()
        {
            if (!Initialized) throw new Exception("Not initialized");
            
            Target.RemoveUnrest(this);
        }

        /// <summary>
        /// Resolve this source. This is already called internally by UnrestActor.
        /// </summary>
        public void Resolve()
        {
            if (!Initialized) throw new Exception("Not initialized");
            
            var prevValue = Target.Value;
            Target.Value += Value;
            AppliedResolvedValue += Mathf.Max(0, Target.Value - prevValue);
            
            // Remove on resolve
            if (RemoveOnResolve) Target.RemoveUnrest(this);
        }
    }
}