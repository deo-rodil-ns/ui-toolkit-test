using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GothicVampire.Game;
using GothicVampire.Villagers;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Jobs
{
    public class JobManager : MonoBehaviour, IFactionService
    {
        [SerializeField] private UnityEvent<Job> _evtJobAdded;
        [SerializeField] private UnityEvent<Job> _evtJobRemoved;
        [SerializeField] private UnityEvent<Job> _evtJobUpdated;

        public IReadOnlyCollection<Job> Jobs => _jobs;

        public UnityEvent<Job> EvtJobAdded => _evtJobAdded;
        public UnityEvent<Job> EvtJobRemoved => _evtJobRemoved;
        public UnityEvent<Job> EvtJobUpdated => _evtJobUpdated;
        
        
        private readonly List<Job> _jobs = new();
        private VillagerManager _villagerManager;
        
        public void AddJob(Job job)
        {
            job.EvtStateChanged?.AddListener(OnJobStateChanged);
            job.EvtReservationChanged?.AddListener(OnJobReservationChanged);
            _jobs.Add(job);
            EvtJobAdded?.Invoke(job);
        }

        public void AddJobs(IEnumerable<Job> jobs)
        {
            jobs.ForEach(AddJob);
        }

        public void RemoveJob(Job job)
        {
            if (!_jobs.Contains(job)) return;
            job.Deactivate(ignoreLock: true, reserve: false);
            _jobs.Remove(job);
            job.EvtStateChanged?.RemoveListener(OnJobStateChanged);
            job.EvtReservationChanged?.RemoveListener(OnJobReservationChanged);
            EvtJobRemoved?.Invoke(job);
        }

        public void RemoveJobs(IEnumerable<Job> jobs)
        {
            jobs.ForEach(RemoveJob);
        }
        
        public void RemoveAllJobs()
        {
            _jobs.ForEach(RemoveJob);
        }

        private void OnJobStateChanged(Job job, JobAssignmentState state) => EvtJobUpdated?.Invoke(job);
        
        private void OnJobReservationChanged(Job job)
        {
            // Try to assign a villager when reserved
            if (!job.Reserved) return;
            
            // Reserve on next frame
            UniTask.NextFrame().ContinueWith(() =>
            {
                if (!job.Reserved) return;
                var villager = _villagerManager.GetUnassignedVillagers(job.RequiredTier).FirstOrDefault();
                if (villager) job.Activate(villager);
            }).Forget();
        }
        
        private void OnVillagerAdded(Villager villager)
        {
            // Try to assign villager to a reserved job
            var job = _jobs.FirstOrDefault(j => j.State == JobAssignmentState.Reserved && j.RequiredTier == villager.Tier);
            job?.Activate(villager);
        }

        #region IFactionService

        public Faction Faction { get; set; }

        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _villagerManager = faction.GetService<VillagerManager>();
            _villagerManager.EvtVillagerAdded.AddListener(OnVillagerAdded);
        }
        
        #endregion
    }
}