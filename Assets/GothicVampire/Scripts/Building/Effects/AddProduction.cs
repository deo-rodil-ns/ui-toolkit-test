using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Jobs;
using GothicVampire.Productions;
using GothicVampire.Unrest;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Buildings.Effects
{
    [System.Serializable]
    public sealed class AddProduction : BuildingEffect
    {
        [SerializeField] private ProductionOrder _order;
        [SerializeField] private bool _lockJobUntilFirstOrderConclusion;
        [SerializeField] private UnrestSource _unrestPerOrder;

        public ProductionOrder Order => _order;
        public IReadOnlyCollection<Currency> InputPerJob => _order.Input;
        public IReadOnlyCollection<Currency> OutputPerJob => _order.Output;
        public IReadOnlyCollection<Currency> InputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> OutputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> NetProjection { get; private set; } = new List<Currency>();
        public float Efficiency => _jobs.Any() 
            ? Mathf.Clamp01((float)_jobs.Count(j => j.Active) / _jobs.Count) 
            : 0f;

        public UnityEvent<AddProduction> EvtProjectionUpdated { get; } = new();

        public override IReadOnlyList<string> DescriptionList 
        {
            get
            {
                var descriptions = new List<string>();
                descriptions.AddRange(_order.Output.Select(c => $"+{c.Value} {c.Type.DisplayName}"));
                descriptions.AddRange(_order.Input.Select(c => $"-{c.Value} {c.Type.DisplayName}"));

                return descriptions;
            }
        }
        
        private JobManager _jobManager;
        private List<ProductionJob> _jobs = new();

        protected override void OnActivate(Building building)
        {
            _jobManager = building.Faction.GetService<JobManager>();
            
            // Create jobs
            RemoveJobs();
            for (var i = 0; i < building.CurrentTier?.JobSlots; i++)
            {
                var job = new ProductionJob(_order)
                {
                    Building = building,
                    RequiredTier = building.CurrentTier?.TierLevel ?? 0,
                    Faction = building.Faction,
                    ShouldLockUntilFirstOrderConclusion = _lockJobUntilFirstOrderConclusion,
                };
                
                // Attach unrest to job if applicable
                if (_unrestPerOrder?.IsValid ?? false) job.Unrest = _unrestPerOrder;
                
                _jobManager.AddJob(job);
                _jobs.Add(job);
                
                // Update projection when order is modified
                job.Order.EvtUpdated.AddListener(o => UpdateProjections());
            }

            UpdateProjections();
        }

        protected override void OnDeactivate(Building building)
        {
            RemoveJobs();
            UpdateProjections();
        }

        protected override void OnJobUpdated(Job job)
        {
            UpdateProjections();
        }

        private void UpdateProjections()
        {
            var maxInput = new CurrencyCollection(_jobs.SelectMany(j => j.Order.Input));
            var maxOutput = new CurrencyCollection(_jobs.SelectMany(j => j.Order.Output));
            
            InputProjection = _jobs
                .SelectMany(j => j.InputProjection)
                .Collate()
                .Select(c => c.WithMax(maxInput.GetValue(c.Type)))
                .ToList();
            OutputProjection = _jobs
                .SelectMany(j => j.OutputProjection)
                .Collate()
                .Select(c => c.WithMax(maxOutput.GetValue(c.Type)))
                .ToList();
            NetProjection = OutputProjection.Separate(InputProjection);
            
            EvtProjectionUpdated?.Invoke(this);
        }

        private void RemoveJobs()
        {
            _jobManager.RemoveJobs(_jobs);
            _jobs.Clear();
        }
    }
}