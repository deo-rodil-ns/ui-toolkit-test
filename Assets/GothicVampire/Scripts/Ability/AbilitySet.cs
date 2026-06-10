using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Set", order = 100)]
    public class AbilitySet : ScriptableObject
    {
        [SerializeField] private AbilityData[] _abilities;
        
        public IReadOnlyCollection<AbilityData> Abilities => _abilities;
    }
}