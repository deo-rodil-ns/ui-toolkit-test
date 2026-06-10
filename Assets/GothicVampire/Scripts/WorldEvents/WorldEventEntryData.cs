using System;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    [Serializable]
    public class WorldEventResult
    {
        public WorldEvent WorldEvent { get; private set; }
        public string EventName => WorldEvent.DisplayName;
        public string Category => WorldEvent.Category;
        public string Description => WorldEvent.Description;
        public int Cycle { get; private set; }
        
        public WorldEventResult(){}

        public WorldEventResult(WorldEvent worldEvent, int cycle)
        {
            WorldEvent = worldEvent;
            Cycle = cycle;
        }
    }
}
