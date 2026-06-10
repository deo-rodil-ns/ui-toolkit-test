using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Technologies
{
    [CreateAssetMenu(menuName = "Technology/Set", order = 10)]
    public class TechnologySet : ScriptableObject
    {
        [SerializeField] private TechnologyData[] _technologies;
        
        public IReadOnlyList<TechnologyData> Technologies => _technologies;
    }
}