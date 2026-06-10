using System.Collections.Generic;
using System.Linq;
using TNRD;
using UnityEngine;

namespace GothicVampire.Technologies
{
    [CreateAssetMenu(menuName = "Technology/Technology", order = 0)]
    public sealed class TechnologyData : ScriptableObject, IPrerequisiteSource, IUnlockable
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField, TextArea] private string _description;
        [SerializeField, TextArea] private string _lore;
        [SerializeField] private TechnologyTag[] _tags;
        
        [Header("Prerequisites")]
        [SerializeField] private PrerequisiteGroup _prerequisites;
        
        [Header("Effects")]
        [SerializeField] private SerializableInterface<IUnlockable>[] _unlockables;
        [SerializeReference, SubclassSelector] private TechnologyEffect[] _effects;

        public string DisplayName => _displayName;
        public string Description => _description;
        public string Lore => _lore;
        public Sprite Icon => _icon;
        public IReadOnlyCollection<TechnologyTag> Tags => _tags;
        public IReadOnlyCollection<IUnlockable> Unlockables => _unlockables.Select(u => u.Value).ToArray();
        public PrerequisiteGroup Prerequisites => _prerequisites;
        public IReadOnlyCollection<TechnologyEffect> Effects => _effects;
        
        string IUnlockable.Name => DisplayName;
        string IUnlockable.Description => Description;
        
        public bool HasTag(TechnologyTag tag) => Tags.Contains(tag);
        public bool HasTags(params TechnologyTag[] tags) => Tags.Any(tags.Contains);
        public bool HasTags(IReadOnlyCollection<TechnologyTag> tags) => Tags.Any(tags.Contains);
    }
}