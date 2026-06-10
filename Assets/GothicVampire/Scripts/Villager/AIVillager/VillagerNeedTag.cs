using UnityEngine;

namespace GothicVampire.Villagers
{
    [CreateAssetMenu(menuName = "Villager/Needs Tag", order = 10)]
    public class VillagerNeedTag : ScriptableObject
    {
        public string DisplayName => name;
    }
}