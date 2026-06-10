using System;
using UnityEngine;
using UnityEngine.Events;

namespace Sylpheed.Timers
{
    [Serializable]
    public class CooldownTimer
    {
        [SerializeField] private float _duration;
        [SerializeField] private bool _readyOnInitialize = true;
        [SerializeField] private UnityEvent<CooldownTimer> _evtReady;
        
        public float Duration => _duration;
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Math.Max(0, _duration - TimeElapsed);
        public float Progress => Mathf.Clamp01(TimeElapsed / _duration);
        public bool Running { get; private set; }
        public bool Ready => TimeElapsed >= _duration;
        public bool Paused { get; set; }

        public UnityEvent<CooldownTimer> EvtReady => _evtReady;
        
        private Action _onReady;
        private bool _hasStartedOnce;

        public static CooldownTimer Create(float duration, bool readyOnInitialize, Action onReady = null)
        {
            var timer = new CooldownTimer
            {
                _duration = duration,
                _readyOnInitialize = readyOnInitialize,
                _onReady = onReady
            };
            
            if (!readyOnInitialize) timer.Start();

            return timer;
        }

        public void Start()
        {
            // Check if we need to handle first run
            if (HandleFirstRun()) return;
            
            // Only reset the time if already running
            if (Running)
            {
                TimeElapsed = 0;
                return;
            }
            
            Start(Duration);
        }
        
        public void Start(float duration, Action onReady = null)
        {
            TimeElapsed = 0;
            _duration = duration;
            _onReady = onReady;
            _ = RunTask();
        }

        public async Awaitable StartAsync()
        {
            await RunTask();
        }

        public async Awaitable StartAsync(float duration)
        {
            _duration = duration;
            await RunTask();
        }

        private bool HandleFirstRun()
        {
            if (_hasStartedOnce) return false;
            _hasStartedOnce = true;

            if (!_readyOnInitialize) return false;
            
            // Set to Ready on first run
            TimeElapsed = Duration;
            return true;
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
                
                // Count until set duration
                TimeElapsed += Time.deltaTime;
                if (TimeElapsed < Duration) continue;
                
                // Finished
                Running = false;
                _onReady?.Invoke();
            }
        }
    }
}