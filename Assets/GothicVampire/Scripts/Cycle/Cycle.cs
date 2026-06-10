using System.Threading;
using GothicVampire.Game;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Cycles
{
    public sealed class Cycle
    {
        public CycleData Data { get; private set; }
        public World World { get; private set; }
        
        public int NumCyclesCompleted { get; private set; }
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Duration - TimeElapsed;
        public float Duration => Data.Duration;
        public float Progress => Mathf.Clamp01(TimeElapsed / Duration);
        
        public UnityEvent<Cycle> EvtCycleCompleted { get; } = new();
        
        public Cycle(World world, CycleData data)
        {
            Data = data;
            World = world;
            _ = RunTask(world.destroyCancellationToken);
        }
        
        private async Awaitable RunTask(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for next frame. Skip if paused.
                await Awaitable.NextFrameAsync(cancellationToken);

                // Count until set duration
                TimeElapsed += Time.deltaTime;
                if (TimeElapsed < Duration) continue;
                
                // Finished
                TimeElapsed = 0f;
                NumCyclesCompleted++;
                
                EvtCycleCompleted.Invoke(this);
            }
        }
    }
}