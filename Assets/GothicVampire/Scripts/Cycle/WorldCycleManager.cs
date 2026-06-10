using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Cycles
{
    public class WorldCycleManager : MonoBehaviour, IWorldService
    {
        [SerializeField] private CycleData[] _data;
        [SerializeField] private CycleData _mainCycle;
        [SerializeField] private CycleBehaviorSet _behaviorSet;

        public UnityEvent<CycleBehaviorSnapshot> EvtCycleResolved { get; } = new();
        
        public IReadOnlyCollection<Cycle> Cycles => _cycles;
        public Cycle MainCycle => GetCycle(_mainCycle);
        
        private List<Cycle> _cycles = new();
        private List<CycleBehavior> _behaviors = new();
        
        #region IWorldService
        public World World { get; set; }

        void IWorldService.OnWorldInitialize(World world)
        {
            // Create production per batch
            _cycles = _data.Select(cycle => new Cycle(world, cycle)).ToList();
            
            // Create runtime instances of behaviors
            if (_behaviorSet)
            {
                _behaviors = _behaviorSet.Behaviors.Select(template =>
                {
                    var behavior = CycleBehavior.Create(template);
                    behavior.EvtCycleResolved.AddListener(snapshot => EvtCycleResolved.Invoke(snapshot));
                    return behavior;
                }).ToList();
            }
        }
        #endregion
        
        public Cycle GetCycle(CycleData data) => _cycles.SingleOrDefault(c => c.Data == data);
        
        private void OnDestroy()
        {
            _behaviors.ForEach(Destroy);
            _behaviors.Clear();
        }
    }
}