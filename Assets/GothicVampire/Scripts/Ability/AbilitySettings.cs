using GothicVampire.Currencies;
using UnityEngine;

namespace GothicVampire.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Settings", order = int.MaxValue)]
    public sealed class AbilitySettings : ScriptableObject
    {
        [SerializeField] private CurrencyType _blood;
        
        public CurrencyType Blood => _blood;
    }
}