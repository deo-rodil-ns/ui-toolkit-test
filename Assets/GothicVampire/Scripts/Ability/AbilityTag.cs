using UnityEngine;

namespace GothicVampire.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Tag", order = 10)]
    public class AbilityTag : ScriptableObject
    {
        public string DisplayName => name;
    }
}