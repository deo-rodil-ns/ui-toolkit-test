using System;
using System.Collections.Generic;
using GothicVampire.Game;
using GothicVampire.Jobs;
using UnityEngine;

namespace GothicVampire.Buildings
{
    public abstract class BuildingEffect
    {
        public Building Building { get; private set; }
        public bool Active { get; private set; }
        public Faction Faction => Building?.Faction;
        
        protected virtual void OnActivate(Building building) { }
        protected virtual void OnDeactivate(Building building) { }
        protected virtual void OnUpdate(Building building, float dt) { }
        protected virtual void OnJobUpdated(Job job) { }
        protected virtual void OnUpgrading(Building building) { }
        protected virtual void OnBuildingRemoved(Building building) { }
        public virtual IReadOnlyList<string> DescriptionList => new List<string>();
        
        public void Activate(Building building)
        {
            if (Active) return;
            
            Active = true;
            Building = building;
            building.JobAssignment?.EvtJobUpdated.AddListener(OnJobUpdated);
            building.EvtUpgrading?.AddListener(OnUpgrading);
            building.EvtRemoved?.AddListener(OnBuildingRemoved);
            
            OnActivate(building);
        }

        public void Deactivate()
        {
            if (!Active) return;
            
            Active = false;
            Building?.JobAssignment?.EvtJobUpdated.RemoveListener(OnJobUpdated);
            Building?.EvtUpgrading?.RemoveListener(OnUpgrading);
            Building?.EvtRemoved?.RemoveListener(OnBuildingRemoved);
            
            OnDeactivate(Building);
        }
        
        public void Update(float dt)
        {
            OnUpdate(Building, dt);
        }
    }
}