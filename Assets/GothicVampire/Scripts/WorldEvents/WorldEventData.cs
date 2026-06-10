using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using GothicVampire.WorldEvents.Effects;
using GothicVampire.WorldEvents.Triggers;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    [CreateAssetMenu(menuName = "World Events/Data", order = 0)]
    public class WorldEventData: ScriptableObject
    {
        [Header("Configuration")]
        [SerializeField] private string _displayName;
        [SerializeField] private WorldEventTags _tag;
        [SerializeField] private bool _showResult;
        [SerializeField, TextArea] private string _description; 

        [Header("Triggers")] 
        [SerializeField] private List<WorldEventTrigger> _triggers;
        [Header("Effects")]
        [SerializeField] private List<WorldEventEffect> _effects;

        public IReadOnlyList<WorldEventTrigger> Triggers => _triggers;
        public IReadOnlyList<WorldEventEffect> Effect => _effects;

        public string Category => _tag.DisplayName;
        public string DisplayName => _displayName;
        public string Description => _description;
    }
}
