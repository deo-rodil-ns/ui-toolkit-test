using System;
using GothicVampire.Buildings;
using GothicVampire.Game;
using UnityEngine.Events;

namespace GothicVampire.Jobs
{
    public abstract class Job
    {
        public Building Building { get; set; }
        public int RequiredTier { get; set; }
        public IAssignee Assignee { get; private set; }
        public Faction Faction { get; set; }
        public bool Active { get; private set; }

        /// <summary>
        /// If true, job cannot be assigned/unassigned.
        /// </summary>
        public bool Locked
        {
            get => _locked;
            set
            {
                if (_locked == value) return;
                _locked = value;
                EvtLockChanged?.Invoke(this);
                EvtStateChanged?.Invoke(this, State);
            }
        }

        public bool Reserved
        {
            get => _reserved;
            set
            {
                if (_reserved == value) return;
                if (Active) return;
                if (Locked) return;
                
                _reserved = value;
                EvtReservationChanged?.Invoke(this);
                EvtStateChanged?.Invoke(this, State);
            }
        }

        public JobAssignmentState State
        {
            get
            {
                // Assigned states
                if (Assignee != null) return JobAssignmentState.Assigned;
                
                // Reserved states
                if (Reserved) return JobAssignmentState.Reserved;
                
                // Unassigned states
                return JobAssignmentState.Unassigned;
            }
        }

        public bool IsAssignableOrReservable
        {
            get
            {
                if (Locked) return false;
                if (Active) return false;
                if (Reserved) return false;
                
                return true;
            }
        }

        public bool IsAssignedOrReserved
        {
            get
            {
                if (Active) return true;
                if (Reserved) return true;
                
                return false;
            }
        }
        
        public UnityEvent<Job> EvtActivated { get; } = new();
        public UnityEvent<Job> EvtDeactivated { get; } = new();
        public UnityEvent<Job> EvtLockChanged { get; } = new();
        public UnityEvent<Job> EvtReservationChanged { get; } = new();
        public UnityEvent<Job, JobAssignmentState> EvtStateChanged { get; } = new();
        
        protected virtual void OnActivate(IAssignee assignee) { }
        protected virtual void OnDeactivate() { }
        /// <summary>
        /// Callback for checking if assignment should be accepted.
        /// </summary>
        /// <param name="assignee"></param>
        /// <returns>True if you want to accept the assignment</returns>
        protected virtual bool ShouldAssign(IAssignee assignee) { return true; }

        private bool _locked;
        private bool _reserved;

        /// <summary>
        /// Assign and activates the job.
        /// </summary>
        /// <param name="assignee"></param>
        /// <param name="ignoreLock"></param>
        /// <exception cref="Exception"></exception>
        public void Activate(IAssignee assignee, bool ignoreLock = false)
        {
            if (Active) return;
            if (assignee == null) return;
            if (!ignoreLock && Locked) return;
            if (!ShouldAssign(assignee)) return;
            
            // Check assignee tier
            if (assignee.Tier != RequiredTier) throw new Exception("Assignee tier doesn't match job");
            
            // Remove reservation if set
            Reserved = false;
            
            Assignee = assignee;
            assignee.Job = this;
            Active = true;
            OnActivate(assignee);
            EvtActivated?.Invoke(this);
            
            EvtStateChanged?.Invoke(this, State);
        }

        /// <summary>
        /// Unassigns and deactivates the job.
        /// </summary>
        public void Deactivate(bool ignoreLock = false, bool reserve = false)
        {
            if (!Active) return;
            if (!ignoreLock && Locked) return;
            
            Active = false;
            
            OnDeactivate();
            if (Assignee != null) Assignee.Job = null;
            Assignee = null;
            EvtDeactivated?.Invoke(this);
            
            EvtStateChanged?.Invoke(this, State);

            // Reserve
            if (reserve) Reserved = true;
        }
    }

    public enum JobAssignmentState
    {
        Assigned, // Assigned
        Reserved, // Unassigned but will be assigned once an assignee is available
        Unassigned, // Unassigned and not reserved
    }
}