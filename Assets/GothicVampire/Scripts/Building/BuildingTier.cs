using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Technologies;
using Sylpheed.Extensions;
using TNRD;
using UnityEngine;

namespace GothicVampire.Buildings
{
    [CreateAssetMenu(menuName = "Building/Tier", order = 1)]
    public class BuildingTier : ScriptableObject, IPrerequisiteSource, IUnlockable
    {
        [SerializeField] private BuildingData _building;
        [SerializeField, Min(1)] private int _tier = 1;
        [SerializeField] private string _nameOverride;
        [SerializeField] private GameObject _model;
        [SerializeField] private GameObject _constructionModel;
        
        [Header("Build/Upgrade")]
        [SerializeField] private float _buildTime;
        [SerializeField] private Currency[] _buildCost;
        
        [Header("Technology")]
        [SerializeField] private PrerequisiteGroup _prerequisites;
        [SerializeField] private SerializableInterface<IUnlockable>[] _unlockablesOnBuild;
        
        [Header("Behavior")]
        [SerializeField] private int _jobSlots;
        [SerializeReference, SubclassSelector] private BuildingEffect _effect;
        private string _name;
        private string _description;

        public BuildingData Building => _building;
        public int TierLevel => _tier;
        public string DisplayName => string.IsNullOrWhiteSpace(_nameOverride) ? Building.DisplayName : _nameOverride;
        public IReadOnlyCollection<Currency> BuildCost => _buildCost;
        public GameObject Model => _model;
        public GameObject ConstructionModel => _constructionModel;
        public int JobSlots => _jobSlots;
        public BuildingEffect Effect => _effect;
        public float BuildTime => _buildTime;
        public PrerequisiteGroup Prerequisites => _prerequisites;
        public IReadOnlyCollection<IUnlockable> UnlockablesOnBuild => _unlockablesOnBuild.Select(u => u.Value).ToList();

        public bool IsLast => Building.Tiers.LastOrDefault() == this;
        public int TierIndex => Building.Tiers.IndexOf(this);

        string IUnlockable.Name => DisplayName;
        string IUnlockable.Description => DisplayName;
    }
}