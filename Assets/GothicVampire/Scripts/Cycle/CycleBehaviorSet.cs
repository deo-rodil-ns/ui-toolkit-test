using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Cycles
{
    [CreateAssetMenu(menuName = "Cycle/Behavior Set", order = 10)]
    public class CycleBehaviorSet : ScriptableObject
    {
        [SerializeField] private CycleBehavior[] _behaviors;
        
        public IReadOnlyCollection<CycleBehavior> Behaviors => _behaviors;
    }
}