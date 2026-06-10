using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Grids;
using UnityEngine;

namespace GothicVampire.Buildings
{
    [CreateAssetMenu(menuName = "Building/Building", order = 0)]
    public sealed class BuildingData : ScriptableObject
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
        [SerializeField] private BuildingHudType _hudType;
        [SerializeField] private Sprite _infoIcon;

        [Header("Construction HUD")]
        [SerializeField] private Sprite _constructionIcon;
        [SerializeField] private Color _constructionCodeColor;

        [Header("Behavior")] 
        [SerializeField] private bool _canAssignJobs;
        [SerializeField] private bool _upgradable;
        [SerializeField] private bool _shouldDeactivateEffectOnUpgrade;
        [SerializeField] private bool _canHouseVillagers;
        [SerializeField] private BuildingTier[] _tiers;
        
        [Header("Tags")]
        [SerializeField] private BuildingTag[] _tags;
        #endregion
        
        public string DisplayName => _displayName;
        public string Description => _description;
        public IReadOnlyList<string> EffectDescription => _tiers.FirstOrDefault()?.Effect?.DescriptionList ?? Array.Empty<string>();
        public string Lore => _lore;
        public GameObject Ghost => _ghost;
        public GridCoord GridSize => _gridSize;
        public bool CanAssignJobs => _canAssignJobs;
        public bool Upgradable => _upgradable;
        public bool ShouldDeactivateEffectOnUpgrade => _shouldDeactivateEffectOnUpgrade;
        public IReadOnlyList<BuildingTier> Tiers => _tiers;
        public BuildingHudType HudType => _hudType;
        public Sprite ConstructionIcon => _constructionIcon;
        public Sprite InfoIcon => _infoIcon;
        public Color ConstructionCodeColor => _constructionCodeColor;
        public float BuildTime => _tiers.FirstOrDefault()?.BuildTime ?? 0f;
        public IReadOnlyCollection<Currency> BuildCost => _tiers.FirstOrDefault()?.BuildCost ?? Array.Empty<Currency>();
        public bool CanHouseVillagers => _canHouseVillagers;
        
        public IReadOnlyCollection<BuildingTag> Tags => _tags;
        public bool HasTag(BuildingTag tag) => _tags.Contains(tag);
        public bool HasTags(IReadOnlyCollection<BuildingTag> tags) => _tags.All(tags.Contains);
    }

    public enum BuildingHudType
    {
        Basic,
        Production,
        VillagerHousing
    }
}