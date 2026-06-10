using System;
using System.Collections.Generic;

namespace GothicVampire.Abilities
{
    /// <summary>
    /// Defines the requirement to use an Ability
    /// </summary>
    [System.Serializable]
    public abstract class AbilityRequirement
    {
        public Ability Ability { get; private set; }
        
        protected virtual void OnInitialize(Ability ability) { }
        protected abstract bool OnCanUse(Ability ability);
        protected virtual void OnConsume(Ability ability) { }
        public virtual IReadOnlyList<string> DescriptionList { get; } = new List<string>();

        private bool _initialized;
        
        public void Initialize(Ability ability)
        {
            if (_initialized) throw new Exception("Already initialized");
            _initialized = true;
            
            Ability = ability;
            OnInitialize(ability);
        }

        /// <summary>
        /// Check if the ability can be used with this requirement.
        /// </summary>
        /// <returns></returns>
        public bool CanUse
        {
            get
            {
                if (!_initialized) return false;
                return OnCanUse(Ability);
            }
        }

        /// <summary>
        /// Consume requirements
        /// </summary>
        /// <returns></returns>
        public bool Consume()
        {
            if (!CanUse) return false;
            OnConsume(Ability);
            return true;
        }
    }
}