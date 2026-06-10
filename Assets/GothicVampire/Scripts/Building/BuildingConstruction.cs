using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Buildings
{
    public sealed class BuildingConstruction : MonoBehaviour
    {
        [SerializeField] private UnityEvent<BuildingConstruction> _evtStarted;
        [SerializeField] private UnityEvent<BuildingConstruction> _evtDone;
        [SerializeField] private UnityEvent<BuildingConstruction> _evtInterrupted;
        [SerializeField] private UnityEvent<BuildingConstruction> _evtProgressUpdated;
        [SerializeField] private UnityEvent<BuildingConstruction> _evtStateUpdated;
        
        public Building Building { get; private set; }
        public float Duration  { get; private set; }
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Duration - TimeElapsed;
        public float Progress => InProgress ? Mathf.Clamp01(TimeElapsed / Duration) : 0f;
        public bool InProgress { get; private set; }
        /// <summary>
        /// No construction. Building is active.
        /// </summary>
        public bool Ready => !InProgress;

        public UnityEvent<BuildingConstruction> EvtStarted => _evtStarted;
        public UnityEvent<BuildingConstruction> EvtDone => _evtDone;
        public UnityEvent<BuildingConstruction> EvtInterrupted => _evtInterrupted;
        public UnityEvent<BuildingConstruction> EvtProgressUpdated => _evtProgressUpdated;
        public UnityEvent<BuildingConstruction> EvtStateUpdated => _evtStateUpdated;

        private CancellationTokenSource _cts;

        /// <summary>
        /// Starts the construction.
        /// </summary>
        /// <param name="building"></param>
        /// <param name="tier"></param>
        /// <returns>Visual model</returns>
        public async UniTask<bool> Execute(Building building, BuildingTier tier)
        {
            if (InProgress) throw new Exception("Already active");
            
            InProgress = true;
            _cts = new CancellationTokenSource();
            
            Building = building;
            Duration = tier.BuildTime;
            TimeElapsed = 0;
            
            // Set the construction model. Remove current model if applicable.
            if (building.Model) Destroy(building.Model);
            building.Model = null;
            if (tier.ConstructionModel) building.Model = Instantiate(tier.ConstructionModel, Building.transform);
            
            EvtStarted?.Invoke(this);
            EvtStateUpdated?.Invoke(this);
            
            // Wait for construction time
            try
            {
                await UniTask.WaitUntil(() =>
                {
                    TimeElapsed += Time.deltaTime;
                    EvtProgressUpdated?.Invoke(this);
                    return TimeElapsed >= Duration;
                }, cancellationToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                InProgress = false;
                TimeElapsed = 0;
                Duration = 0;
                Destroy(building.Model);
                building.Model = null;
                
                return false;
            }
            
            // Cancel if building was destroyed.
            if (!building) return false; 
            
            // Destroy construction model
            if (building.Model) Destroy(building.Model);
            
            // Update model to the selected tier
            if (tier.Model) building.Model = Instantiate(tier.Model, Building.transform);
            
            InProgress = false;
            EvtDone?.Invoke(this);
            EvtStateUpdated?.Invoke(this);
            
            TimeElapsed = 0;
            Duration = 0;
            _cts = null;
            
            return true;
        }

        public void Interrupt()
        {
            if (!InProgress) return;
            
            _cts?.Cancel();
            _cts = null;
            InProgress = false;
            TimeElapsed = 0;
            Duration = 0;
            
            EvtInterrupted?.Invoke(this);
            EvtStateUpdated?.Invoke(this);
        }
    }
}