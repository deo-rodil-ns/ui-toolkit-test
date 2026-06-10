using GothicVampire.Currencies;
using GothicVampire.Roads.Effects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.Roads
{
    [System.Serializable]
    public class RoadTier
    {
        [SerializeField] private string _nameOverride;
        [SerializeField] private List<DirectionMaterials> _roadMaterials;
        [SerializeField] private Currency[] _buildCost;
        [SerializeReference, SubclassSelector] private RoadEffect _effect;

        public string NameOverride => _nameOverride;
        public IReadOnlyCollection<Currency> BuildCost => _buildCost;
        public int MaterialCount => _roadMaterials?.Count ?? 0;

        public RoadEffect Effect => _effect;

        public Material GetMaterial(RoadConnection index)
        {
            if (_roadMaterials == null || _roadMaterials.Count == 0)
                return null;

            DirectionMaterials material = _roadMaterials.FirstOrDefault(x => x.Connection == index);

            if(material == null) return null;

            return material.Material;
        }
    }

    public enum RoadConnection
    {
        NorthSouth,
        NorthSouthEast,
        NorthSouthWest,
        EastWest,
        EastWestNorth,
        EastWestSouth,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest,
        Cross,
    }   
    
    [System.Serializable]
    public class DirectionMaterials
    {
        [SerializeField] private RoadConnection _connection;
        [SerializeField] private Material _material;

        public RoadConnection Connection => _connection;
        public Material Material => _material;
    }
}
