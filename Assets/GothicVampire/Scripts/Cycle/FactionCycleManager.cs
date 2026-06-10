using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Cycles
{
    public class FactionCycleManager : MonoBehaviour, IFactionService
    {
        [SerializeField] private CycleBehaviorSet _behaviorSet;

        public UnityEvent<CycleBehaviorSnapshot> EvtCycleResolved { get; } = new();

        private List<CycleBehavior> _behaviors = new();
        
        #region IFactionService

        public Faction Faction { get; set; }
        void IFactionService.OnFactionInitialize(Faction faction)
        {
            // Create runtime instances of behaviors
            _behaviors = _behaviorSet.Behaviors.Select(template =>
            {
                var behavior = CycleBehavior.Create(template, faction);
                behavior.EvtCycleResolved.AddListener(snapshot => EvtCycleResolved.Invoke(snapshot));
                return behavior;
            }).ToList();
        }

        #endregion

        private void OnDestroy()
        {
            _behaviors.ForEach(Destroy);
            _behaviors.Clear();
        }
    }
}