using System;
using UnityEngine;

namespace GothicVampire.Roads.Effects
{
    [System.Serializable]
    public sealed class BasicRoadEffect : RoadEffect
    {
        [SerializeField] private RoadData _roadData;

        private RoadManager _roadManager;
        public RoadData RoadData => _roadData;

        protected override void OnActivate(Road road)
        {
            _roadData = Road.Data;
            _roadManager = road.Faction.GetService<RoadManager>();
        }

        protected override void OnDeactivate(Road road)
        {

        }
    }
}
