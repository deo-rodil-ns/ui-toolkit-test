using System;
using GothicVampire.Grids;
using GothicVampire.Jobs;
using Sylpheed.Core;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Villagers
{
    public sealed class Villager : MonoBehaviour, IAssignee
    {
        [SerializeField] private VillagerSettings _settings;
        [SerializeField] private UnityEvent<Villager> _evtDataUpdated;
        [SerializeField] private UnityEvent<Villager> _evtJobUpdated;
        
        public VillagerData Data { get; private set; }
        /// <summary>
        /// Metadata on where the villager was created from
        /// </summary>
        public VillagerSource Source { get; private set; }
        public VillagerIdentity Identity { get; private set; }
        
        public int Tier => Data?.Tier ?? 0;

        public Job Job
        {
            get => _job;
            set
            {
                if (_job == value) return;
                _job = value;
                EvtJobUpdated?.Invoke(this);
            }
        }

        public GameObject Model { get; private set; }
        
        public UnityEvent<Villager> EvtDataUpdated => _evtDataUpdated;
        public UnityEvent<Villager> EvtJobUpdated => _evtJobUpdated;
        public UnityEvent<Villager> EvtDestroying { get; } = new();

        private bool _initialized;
        private Job _job;

        public void Initialize(VillagerData data, VillagerSource source)
        {
            if (_initialized) return;
            _initialized = true;
            
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Source.AddVillager(this);
            
            Identity = _settings.GenerateIdentity();
            UpdateData(data);
            
            // TODO: Temporary code. Remove this later.
            SetInitialPosition_Test();

            var villagerBrain = GetComponent<VillagerBrain>();
            villagerBrain.Initialize(data);
        }

        private void OnDestroy()
        {
            if (!_initialized) return;

            Job?.Deactivate(ignoreLock: true, reserve: true);
            Source?.RemoveVillager(this);
            
            EvtDestroying?.Invoke(this);
        }

        private void SetInitialPosition_Test()
        {
            // Set position near the source building
            var buildingService = ServiceLocator.Get<IBuildingService>();
            var targetGrid = new GridCoord(Source.Building.GridPosition.x - 1, Source.Building.GridPosition.y);
            var worldPos = buildingService.GridToWorldPosition(targetGrid);
            transform.position = worldPos;
        }
        
        public void UpdateData(VillagerData data)
        {
            if (Data == data) return;
            
            // Update data and model
            Data = data ?? throw new ArgumentNullException(nameof(data));
            UpdateModel();
            
            // Deactivate current job if tier no longer matches
            if (Job?.RequiredTier != Tier) Job?.Deactivate(ignoreLock: true, reserve: true);
            
            EvtDataUpdated?.Invoke(this);
        }

        private void UpdateModel()
        {
            // Destroy previous model
            if (Model) Destroy(Model);
            Model = null;
            
            // Create model based on assigned building
            var modelPrefab = Job?.Building ? Data.GetModel(Job.Building.Data) : Data.DefaultModel;
            if (modelPrefab) Model = Instantiate(modelPrefab, transform);
        }
    }
}