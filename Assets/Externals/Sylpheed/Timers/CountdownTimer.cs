using System;
using UnityEngine;

namespace Sylpheed.Timers
{
    [Serializable]
    public class CountdownTimer
    {
        [SerializeField] private float _duration;
        
        public float Duration => _duration;
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Math.Max(0, _duration - TimeElapsed);
        public float Progress => Mathf.Clamp01(TimeElapsed / _duration);
        public bool Running { get; private set; }
        public bool Paused { get; set; }
        
        private bool _isAsync;
        private Action _onFinished;
        
        private CountdownTimer() { } // Prevent instantiation

        public static CountdownTimer Create(float duration, Action onFinished = null)
        {
            var timer = new CountdownTimer()
            {
                _duration = duration,
                _onFinished = onFinished
            };

            return timer;
        }
        
        public static CountdownTimer Run(float duration, Action onFinished = null)
        {
            var timer = Create(duration, onFinished);
            _ = timer.RunTask();
            
            return timer;
        }

        public void Start(Action onFinished = null) => Start(_duration, onFinished);

        public void Start(float duration, Action onFinished = null)
        {
            _duration = duration;
            _onFinished = onFinished;
            _ = RunTask();
        }
        
        public async Awaitable StartAsync() => await StartAsync(_duration);

        public async Awaitable StartAsync(float duration)
        {
            var timer = new CountdownTimer()
            {
                _duration = duration,
            };
            await timer.RunTask();
        }

        private async Awaitable RunTask()
        {
            if (Running) return;
            Running = true;
            
            while (Running)
            {
                // Wait for next frame. Skip if paused.
                await Awaitable.NextFrameAsync();
                if (Paused) continue;
                
                // Count
                TimeElapsed += Time.deltaTime;
                if (!(TimeElapsed >= _duration)) continue;
                
                // Finished
                Running = false;
                _onFinished?.Invoke();
            }
        }
    }
}