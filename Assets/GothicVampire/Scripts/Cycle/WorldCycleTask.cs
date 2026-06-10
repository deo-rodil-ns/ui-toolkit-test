using GothicVampire.Game;

namespace GothicVampire.Cycles
{
    public abstract class WorldCycleTask : CycleTask
    {
        protected virtual void OnInitialize(World world) { } 
        protected virtual void OnDestroy(World world) { }
        protected virtual void OnExecute(Cycle cycle, World world, CycleBehaviorSnapshot snapshot) { }

        protected sealed override void OnInitialize(Cycle cycle, Faction faction)
        {
            OnInitialize(cycle.World);
        }

        protected sealed override void OnExecute(CycleBehaviorSnapshot snapshot)
        {
            OnExecute(snapshot.Cycle, snapshot.Cycle.World, snapshot);
        }

        protected sealed override void OnDestroy()
        {
            OnDestroy(Cycle.World);
        }
    }
}