using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GothicVampire.Game;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Technologies
{
    [System.Serializable]
    public abstract class Prerequisite
    {
        [Tooltip("Set this to any value to evaluate prerequisite using progress.")]
        [SerializeField, Min(0)] private float _targetCount;
        [Tooltip("Set to 0 for every frame. Set to negative value to disable.")]
        [SerializeField] private float _resolveInterval = -1f;
        [Tooltip("Tokenized description. Leave empty if description is already generated completely from code.")]
        [SerializeField, TextArea] private string _descriptionTemplate;

        public bool Satisfied { get; private set; }
        public Faction Faction { get; private set; }
        public string Description => OnBuildDescription(_descriptionTemplate);
        
        /// <summary>
        /// Prerequisites that do not care about progress always has progress of 0.
        /// </summary>
        public float CurrentProgress { get; private set; }
        public float MaxProgress => _targetCount;
        public float ProgressRate => Mathf.Clamp01(CurrentProgress / Mathf.Max(MaxProgress, 1));
        public bool HasProgression => _targetCount > 0;
        
        public bool HasResolvedOnce { get; private set; }

        public UnityEvent<Prerequisite> EvtSatisfied { get; } = new();
        public UnityEvent<Prerequisite> EvtProgressUpdated { get; } = new();

        protected virtual void OnInitialize(Faction faction) { }
        protected virtual void OnDestroy() { }
        /// <summary>
        /// By default, prerequisite is only checked using progress.
        /// Override this for a more custom behavior or if you don't care about progress count.
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnShouldSatisfy() => CurrentProgress >= _targetCount;
        /// <summary>
        /// This is used to update progress and potentially satisfy this prerequisite.
        /// If this prerequisite doesn't have a target count, returning a positive value can satisfy this prerequisite (assuming OnShouldSatisfy() also returns true)
        /// </summary>
        /// <returns>Current progress count</returns>
        protected virtual float OnResolveProgress() => 0f;
        protected virtual string OnBuildDescription(string template) => _descriptionTemplate; 

        private bool _initialized;
        private CancellationTokenSource _cts;
        
        public void Initialize(Faction faction, bool handleFirstResolve = true)
        {
            if (_initialized) return;
            _initialized = true;
            
            Faction = faction;
            OnInitialize(faction);
            
            // Loop resolve
            if (!Satisfied && _resolveInterval >= 0)
            {
                _cts = new CancellationTokenSource();
                ResolveTask(_cts.Token).Forget();
            }

            // Resolve on next frame
            if (handleFirstResolve) DelayedResolve().Forget();
        }

        // TODO: It seems like we don't need this anymore. Revisit later.
        private async UniTaskVoid DelayedResolve()
        {
            // Resolve on next frame and wait for unlockable resolver
            UniTask.DelayFrame(1).ContinueWith(Resolve).Forget();
            await UniTask.WaitUntil(() => Faction.GetUnlockableResolver() != null);
            
            Resolve();
        }

        ~Prerequisite()
        {
            if (!_initialized) return;
            OnDestroy();
            _cts?.Cancel();
            _cts = null;
        }

        /// <summary>
        /// Check if we can satisfy this prerequisite. Internally, you can use event handlers to call this function.
        /// This is automatically called on the next frame after Initialize()
        /// </summary>
        /// <returns>True if satisfied.</returns>
        public bool Resolve()
        {
            HasResolvedOnce = true;
            
            // Update progress
            var prevProgress = CurrentProgress;
            CurrentProgress = HasProgression ? OnResolveProgress() : 0f;
            if (!Mathf.Approximately(prevProgress, CurrentProgress)) EvtProgressUpdated?.Invoke(this);

            // Skip if already satisfied
            if (Satisfied) return true;
            
            // Check if this prerequisite can be satisfied
            if (HasProgression && CurrentProgress < _targetCount) return false;
            if (!OnShouldSatisfy()) return false;
            
            Satisfy();
            return true;
        }

        /// <summary>
        /// Satisfy the prerequisite. This can be called immediately when conditions are met.
        /// </summary>
        protected void Satisfy()
        {
            // Skip if already satisfied
            if (Satisfied) return;
            
            Satisfied = true;
            _cts?.Cancel();
            _cts = null;
            
            EvtProgressUpdated?.Invoke(this);
            EvtSatisfied?.Invoke(this);
        }

        private async UniTaskVoid ResolveTask(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.WaitUntil(() => HasResolvedOnce, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            
            await UniTask.Yield();
            while (!Satisfied && Faction)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_resolveInterval), cancellationToken: cancellationToken);
                    Resolve();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}