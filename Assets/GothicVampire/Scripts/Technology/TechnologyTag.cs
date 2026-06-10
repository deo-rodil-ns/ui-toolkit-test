using UnityEngine;

namespace GothicVampire.Technologies
{
    [CreateAssetMenu(menuName = "Technology/Tag", order = 10)]
    public class TechnologyTag : ScriptableObject
    {
        public string DisplayName => name;
    }
}