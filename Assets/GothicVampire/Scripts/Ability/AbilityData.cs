using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using Sylpheed.Timers;
using UnityEngine;

namespace GothicVampire.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Ability", order = 0)]
    public sealed class AbilityData : ScriptableObject, IPrerequisiteSource, IUnlockable
    {
        [Header("General")]
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private AbilityTag[] _tags;

        [Header("Requirements")] 
        [SerializeField] private CooldownTimer _cooldown;
        [SerializeField] private bool _consumeRequirementsOnTargeting;
        [SerializeField] private PrerequisiteGroup _prerequisites;
        [SerializeReference, SubclassSelector] private AbilityRequirement[] _requirements;

        [Header("Targeting")] 
        [SerializeReference, SubclassSelector] private TargetingScheme _targetingScheme;

        [Header("Effect")]
        [SerializeReference, SubclassSelector] private AbilityEffect _effect;
        
        public string DisplayName => _displayName;
        public string Description => _effect?.Description ?? string.Empty;
        public Sprite Icon => _icon;
        public CooldownTimer Cooldown => _cooldown;
        public IReadOnlyList<AbilityRequirement> Requirements => _requirements;
        public bool ConsumeRequirementsOnTargeting => _consumeRequirementsOnTargeting;
        public PrerequisiteGroup Prerequisites => _prerequisites;
        public TargetingScheme TargetingScheme => _targetingScheme;
        public AbilityEffect Effect => _effect;
        
        string IUnlockable.Name => _displayName;
        string IUnlockable.Description => Description;
        
        public IReadOnlyList<AbilityTag> Tags => _tags;
        public bool HasTag(AbilityTag tag) => Tags.Contains(tag);
        public bool HasTags(params AbilityTag[] tags) => Tags.Any(tags.Contains);
        public bool HasTags(IReadOnlyCollection<AbilityTag> tags) => Tags.Any(tags.Contains);
    }
}