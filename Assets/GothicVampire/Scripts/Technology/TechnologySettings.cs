using System.Collections.Generic;
using System.Linq;
using TNRD;
using UnityEngine;

namespace GothicVampire.Technologies
{
    [CreateAssetMenu(menuName = "Technology/Settings", order = int.MaxValue)]
    public sealed class TechnologySettings : ScriptableObject
    {
        [SerializeField] private TechnologyData[] _technologies;
        [SerializeField] private SerializableInterface<IUnlockable>[] _unlockablesToPreUnlock;
        [SerializeField] private SerializableInterface<IUnlockable>[] _unlockables;
        
        public IReadOnlyCollection<TechnologyData> Technologies => _technologies;

        public IReadOnlyCollection<IUnlockable> UnlockablesToPreUnlock => _unlockablesToPreUnlock.Select(u => u.Value).ToArray();
        public IReadOnlyCollection<IUnlockable> Unlockables => _unlockables.Select(u => u.Value).ToArray();
    }
}