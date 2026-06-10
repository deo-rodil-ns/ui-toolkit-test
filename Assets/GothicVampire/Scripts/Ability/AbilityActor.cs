using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Abilities
{
    public sealed class AbilityActor : MonoBehaviour
    {
        [SerializeField] private AbilitySet _set;
        [SerializeField] private AbilitySettings _settings;

        [Header("Events")] 
        [SerializeField] private UnityEvent<Ability> _evtAbilityAdded;
        [SerializeField] private UnityEvent<Ability> _evtAbilityRemoved;
        [SerializeField] private UnityEvent _evtAbilitiesUpdated;
        
        public IReadOnlyCollection<Ability> Abilities => _abilities;
        public AbilitySettings Settings => _settings;
        public Faction Faction { get; set; } // Internally set by faction
        
        public UnityEvent<Ability> EvtAbilityAdded => _evtAbilityAdded;
        public UnityEvent<Ability> EvtAbilityRemoved => _evtAbilityRemoved;
        public UnityEvent EvtAbilitiesUpdated => _evtAbilitiesUpdated;
        
        private readonly List<Ability> _abilities = new();
        private bool _wasUpdated;

        private void Start()
        {
            _set.Abilities.ForEach(AddAbility);
        }

        public void AddAbility(AbilityData data)
        {
            var ability = new Ability(data, this);
            _abilities.Add(ability);
            EvtAbilityAdded?.Invoke(ability);
            _wasUpdated = true;
        }
        
        public void RemoveAbility(Ability ability)
        {
            if (!_abilities.Remove(ability)) return;
            
            EvtAbilityRemoved?.Invoke(ability);
            _wasUpdated = true;
        }

        /// <summary>
        /// Remove all abilities derived from the same AbilityData
        /// </summary>
        /// <param name="data"></param>
        public void RemoveAbility(AbilityData data)
        {
            var abilities = _abilities.Where(a => a.Data == data).ToList();
            abilities.ForEach(RemoveAbility);
        }

        public Ability GetAbility(AbilityData data)
        {
            return _abilities.FirstOrDefault(a => a.Data == data);
        }

        private void LateUpdate()
        {
            if (!_wasUpdated) return;
            _wasUpdated = false;
            
            EvtAbilitiesUpdated?.Invoke();
        }
    }
}