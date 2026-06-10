using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.Currencies
{
    [CreateAssetMenu(menuName = "Currency/Type", order = 0)]
    public sealed class CurrencyType : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _backgroundColor;
        [SerializeField] private string[] _tags;
        
        public string Id => name;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Color BackgroundColor => _backgroundColor;
        public IReadOnlyCollection<string> Tags => _tags;

        public Currency CreateCurrency(float value = 0f, float max = 0f)
        {
            return new Currency(this, value, max);
        }

        public bool HasTag(string tag) => _tags.Any(t => string.Equals(t, tag));
    }
}