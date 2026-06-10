using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Roads.Effects
{
    [System.Serializable]
    public abstract class RoadEffect
    {
        public Road Road { get; private set; }
        public bool Active { get; private set; }
        
        public virtual IReadOnlyList<string> EffectDescription => new List<string>();

        protected virtual void OnActivate(Road road) 
        { 
        
        }
        
        protected virtual void OnDeactivate(Road road)
        {

        }

        protected virtual void OnUpgrading(Road road) 
        { 
        
        }


        public void Activate(Road road)
        {
            if (Active) return;

            Active = true;
            Road = road;

            OnActivate(road);
        }

        public void Deactivate()
        {
            if(!Active) return;

            Active = false;
        }
    }
}
