using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Jobs;
using GothicVampire.Villagers;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Buildings
{
    public sealed class BuildingJobAssignment
    {
        public Building Building { get; private set; }
        /// <summary>
        /// Number of villagers assigned to this building.
        /// </summary>
        public int NumAssignedSlots => _jobs.Count(j => j.Active);
        /// <summary>
        /// Remaining number of slots regardless of villager availability.
        /// </summary>
        public int NumUnassignedSlots => Building.CurrentTier.JobSlots - NumAssignedSlots;
        /// <summary>
        /// Maximum number of slots regardless of villager availability.
        /// </summary>
        public int MaxSlots => Building.CurrentTier.JobSlots;
        /// <summary>
        /// Maximum number of slots that can be assigned based on current available villagers.
        /// </summary>
        public int MaxAssignableSlots => Math.Min(NumAssignedSlots + RemainingAssignableSlots, MaxSlots);
        /// <summary>
        /// Remaining slots that can be assigned based on current available villagers.
        /// </summary>
        public int RemainingAssignableSlots => Math.Min(_villagerManager.GetUnassignedVillagers(Building.CurrentTier?.TierLevel ?? 0).Count, NumUnassignedSlots);
        /// <summary>
        /// Number of slots that are reserved.
        /// </summary>
        public int NumReservedSlots => _jobs.Count(j => j.Reserved);
        /// <summary>
        /// Number of slots that are locked.
        /// </summary>
        public int NumLockedSlots => _jobs.Count(j => j.Locked);
        /// <summary>
        /// Rate of assignment based on max slots.
        /// </summary>
        public float AssignmentRate => Mathf.Clamp01((float)NumAssignedSlots / MaxSlots);
        /// <summary>
        /// Jobs provided by this building.
        /// </summary>
        public IReadOnlyCollection<Job> Jobs => _jobs;
        
        public bool CanAssign => _jobs.Any(j => j.IsAssignableOrReservable);
        public bool CanUnassign => _jobs.Any(j => j.IsAssignedOrReserved && !j.Locked);

        public UnityEvent<Job> EvtJobUpdated { get; } = new();
        
        
        private readonly VillagerManager _villagerManager;
        private readonly JobManager _jobManager;
        private readonly List<Job> _jobs = new();

        public BuildingJobAssignment(Building building)
        {
            Building = building;
            _villagerManager = building.Faction.GetService<VillagerManager>();
            _jobManager = building.Faction.GetService<JobManager>();
            
            _jobManager.EvtJobAdded.AddListener(OnJobAdded);
            _jobManager.EvtJobRemoved.AddListener(OnJobRemoved);
        }

        ~BuildingJobAssignment()
        {
            _jobManager.EvtJobAdded.RemoveListener(OnJobAdded);
            _jobManager.EvtJobRemoved.RemoveListener(OnJobRemoved);
        }
        
        /// <summary>
        /// Assign an additional count of villagers to jobs. If it exceeds the remaining slot, only fill the remaining slot.
        /// </summary>
        /// <param name="count"></param>
        public void Assign(int count = 1)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            
            // Cap to remaining slots
            var toAssign = System.Math.Min(NumUnassignedSlots, count);
            if (toAssign < 1) return;
            var jobs = _jobs.Where(j => j.IsAssignableOrReservable).Take(toAssign).ToList();
            
            // Get available villagers
            var unassignedVillagers = _villagerManager.GetUnassignedVillagers(Building.CurrentTier?.TierLevel ?? 0).Take(toAssign).ToList();
            
            // Assign jobs to villagers or reserve if there are no villagers left
            for (var i = 0; i < jobs.Count; i++)
            {
                var job = jobs[i];
                
                // Try to get an available villager
                var villager = unassignedVillagers.ElementAtOrDefault(i);
                
                // Activate the job if there's a villager. Else, reserve the job.
                if (villager) job.Activate(villager);
                else job.Reserved = true;
            }
        }
        
        /// <summary>
        /// Assign as many villagers possible to fill the remaining slots
        /// </summary>
        public void AssignAll()
        {
            Assign(NumUnassignedSlots);
        }

        /// <summary>
        /// Unassign a specific count of villagers from jobs.
        /// </summary>
        /// <param name="count"></param>
        public void Unassign(int count = 1)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            
            // Get enough jobs that can be unassigned
            var jobsToUnassign = _jobs.Where(j => j.IsAssignedOrReserved && !j.Locked)
                .OrderByDescending(j => j.Reserved) // Prioritize unassigning reservations
                .Take(count)
                .ToList();
            if (!jobsToUnassign.Any()) return;
            
            // Deactivate or remove reservation of jobs
            foreach (var job in jobsToUnassign)
            {
                if (job.Reserved) job.Reserved = false;
                else job.Deactivate();
            }
        }

        /// <summary>
        /// Unassign all villagers
        /// </summary>
        public void UnassignAll()
        {
            Unassign(NumUnassignedSlots);
        }
        
        private void OnJobAdded(Job job)
        {
            if (job.Building != Building) return;
            _jobs.Add(job);

            // Listen to job state changes. Manually call event at first.
            OnJobStateChanged(job, job.State);
            job.EvtStateChanged.AddListener(OnJobStateChanged);
        }

        private void OnJobRemoved(Job job)
        {
            if (job.Building != Building) return;
            _jobs.Remove(job);
            
            job.EvtStateChanged.RemoveListener(OnJobStateChanged);
        }

        private void OnJobStateChanged(Job job, JobAssignmentState state)
        {
            EvtJobUpdated?.Invoke(job);
        }
    }
}