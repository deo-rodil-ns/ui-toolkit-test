using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Technologies;
using UnityEngine;
using GothicVampire.Villagers.Actions;

namespace GothicVampire.Villagers
{
    [CreateAssetMenu(menuName = "Villager/Villager", order = 0)]
    public sealed class VillagerData : ScriptableObject, IUnlockable
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _tier;
        [SerializeField] private Currency[] _upkeepCost;
        
        [Header("Visual")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _defaultModel;
        [Tooltip("Set a specific model for building")]
        [SerializeField] private JobModel[] _models;

        [Header("Behavior")] 
        [SerializeField] private float _productionOutputModifier = 1f;
        [Header("Needs")]
        [SerializeField] private List<VillagerNeedType > _villagerNeeds;
        [Header("Actions")]
        [SerializeField] private List<VillagerAction> _villagerActions;

        public string DisplayName => _displayName;
        public int Tier => _tier;
        public IReadOnlyCollection<Currency> UpkeepCost => _upkeepCost;
        public Sprite Icon => _icon;
        public GameObject DefaultModel => _defaultModel;
        public List<VillagerNeedType> VillagerNeeds => _villagerNeeds;
        public List<VillagerAction> VillagerActions => _villagerActions;
        
        string IUnlockable.Name => DisplayName;
        string IUnlockable.Description => DisplayName;

        /// <summary>
        /// Get a model based on building. Returns default model if not available.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        public GameObject GetModel(BuildingData building) 
            => _models.SingleOrDefault(jm => jm._building == building)?._model ?? _defaultModel;
        
        public float ProductionOutputModifier => _productionOutputModifier;
    }
    
    [System.Serializable]
    internal class JobModel
    {
        [SerializeField] internal BuildingData _building;
        [SerializeField] internal GameObject _model;
    }
}