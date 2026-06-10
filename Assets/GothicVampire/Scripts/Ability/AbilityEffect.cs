using System;
using System.Collections.Generic;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.Abilities
{
    [System.Serializable]
    public abstract class AbilityEffect
    {
        [Header("General")] 
        [SerializeField, TextArea] private string _descriptionTemplate;
        [SerializeField] private bool _handleMultipleTargetsIndividually;
        
        public Ability Ability { get; private set; }
        public string Description => OnBuildDescription(_descriptionTemplate);
        public bool HandleMultipleTargetsIndividually => _handleMultipleTargetsIndividually;

        public virtual IReadOnlyList<string> DescriptionList => new List<string>();
        protected virtual string OnBuildDescription(string template) => _descriptionTemplate;
        protected virtual void OnInitialize(Ability ability) { }
        protected virtual void OnDestroy(Ability ability) { }
        protected virtual void OnActivate(Ability ability, ITargetable target) { }
        protected virtual void OnActivate(Ability ability, IReadOnlyCollection<ITargetable> targets) { }
        
        private bool _initialized;

        public void Initialize(Ability ability)
        {
            if (_initialized) throw new Exception("Already initialized");
            _initialized = true;
            
            Ability = ability;
            OnInitialize(ability);
        }

        public void Activate(Ability ability, ITargetable target)
        {
            if (!_initialized) throw new Exception("Not initialized");
            OnActivate(ability, target);
        }

        public void Activate(Ability ability, IReadOnlyCollection<ITargetable> targets)
        {
            if (!_initialized) throw new Exception("Not initialized");
            if (HandleMultipleTargetsIndividually) targets.ForEach(target => OnActivate(ability, target));
            else OnActivate(ability, targets);
        }
        
        ~AbilityEffect()
        {
            if (!_initialized) return;
            OnDestroy(Ability);
        }

        protected void Log(string message)
        {
            Debug.Log($"[{nameof(Ability)}] [{GetType().Name}] {message}");
        }
    }
}