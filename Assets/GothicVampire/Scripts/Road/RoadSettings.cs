using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Roads
{
    [CreateAssetMenu(menuName = "Road/Settings", order = 100)]
    public class RoadSettings : ScriptableObject
    {
        [SerializeField] private float _sellRate = 0.5f;
        [SerializeField] private Road _roadPrefab;
        [SerializeField] private RoadData[] _purchasableRoads;

        public float SellRate => _sellRate;
        public Road RoadPrefab => _roadPrefab;
        public IReadOnlyList<RoadData> PurchasableRoads => _purchasableRoads;
    }
}
