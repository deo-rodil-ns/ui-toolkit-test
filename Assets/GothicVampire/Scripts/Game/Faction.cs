using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Abilities;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.Game
{
    public class Faction : MonoBehaviour, ITargetable
    {
        [SerializeField] private AbilityActor _abilityActor;
        
        public World World { get; private set; }
        public AbilityActor AbilityActor => _abilityActor;
        
        private readonly List<IFactionService> _services = new();

        public void Initialize(World world)
        {
            World = world;
            
            // Initialize AbilityActor
            if (AbilityActor != null) AbilityActor.Faction = this;
            
            // Find faction services
            var services = GetComponentsInChildren<IFactionService>();
            _services.AddRange(services);
            
            // Initialize services. Assign faction reference first before calling initialize to prevent null reference.
            services.ForEach(service => service.Faction = this);
            services.ForEach(service => service.OnFactionInitialize(this));
        }
        
        private void Awake()
        {
            
        }

        /// <summary>
        /// Gets a faction-wide service. To add a new service, add a child gameobject with an attached IFactionService MonoBehaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() where T : IFactionService => _services.OfType<T>().SingleOrDefault();
    }
}