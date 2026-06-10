using System;
using GothicVampire.Currencies;
using GothicVampire.Grids;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using UnityEngine;

namespace GothicVampire.Roads
{
    [CreateAssetMenu(menuName = "Road/Road", order = 0)]
    public class RoadData : ScriptableObject, IUnlockable
    {
        #region Inspector
        [Header("General")]
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea] private string _description;
        [SerializeField] [TextArea] private string _lore;

        [Header("Placement & Purchasing")]
        [SerializeField] private GameObject _ghost;
        [SerializeField] private Vector2 _gridSize = new(1f, 1f);

        [Header("UI")]
        [SerializeField] private Sprite _infoIcon;

        [Header("Construction HUD")]
        [SerializeField] private Sprite _constructionIcon;
        [SerializeField] private Color _constructionCodeColor;

        [Header("Behavior")]
        [SerializeField] private bool _upgradable;
        [SerializeField] private RoadTier[] _tiers;
        #endregion
        
        public GameObject Ghost => _ghost;
        public GridCoord GridSize => _gridSize;
        public string DisplayName => _displayName;
        public string Description => _description;
        public IReadOnlyList<string> EffectDescription => _tiers.FirstOrDefault()?.Effect?.EffectDescription ?? Array.Empty<string>();
        public string Lore => _lore;
        public bool Upgradable => _upgradable;
        public IReadOnlyList<RoadTier> Tiers => _tiers;
        public Sprite ConstructionIcon => _constructionIcon;
        public Sprite InfoIcon => _infoIcon;
        public Color ConstructionCodeColor => _constructionCodeColor;
        public IReadOnlyCollection<Currency> BuildCost => _tiers.FirstOrDefault()?.BuildCost ?? Array.Empty<Currency>();
        
        string IUnlockable.Name => DisplayName;
        string IUnlockable.Description => Description;
    }
}
