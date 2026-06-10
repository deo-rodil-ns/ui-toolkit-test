using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Buildings
{
    [CreateAssetMenu(menuName = "Building/Settings", order = 100)]
    public sealed class BuildingSettings : ScriptableObject
    {
        [SerializeField] private float _sellRate = 0.5f;
        [SerializeField] private Building _buildingPrefab;
        [SerializeField] private BuildingData[] _purchasableBuildings;
        
        public float SellRate => _sellRate;
        public Building BuildingPrefab => _buildingPrefab;
        public IReadOnlyList<BuildingData> PurchasableBuildings => _purchasableBuildings;
    }
}