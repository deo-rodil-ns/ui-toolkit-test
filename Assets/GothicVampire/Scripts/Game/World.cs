using System;
using System.Collections.Generic;
using System.Linq;
using Sylpheed.Core;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.Game
{
    public class World : MonoBehaviour
    {
        [SerializeField] private Faction _player;
        
        public Faction Player => _player;
        public static World Current => ServiceLocator.Get<World>();
        
        private readonly List<IWorldService> _services = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
            
            // Find world services
            var services = GetComponentsInChildren<IWorldService>();
            _services.AddRange(services);
            
            // Initialize services. Assign world reference first before calling initialize to prevent null reference.
            services.ForEach(service => service.World = this);
            services.ForEach(service => service.OnWorldInitialize(this));
            
            // Initialize factions
            var factions = FindObjectsByType<Faction>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            factions.ForEach(f => f.Initialize(this));
        }

        private void OnDestroy()
        {
            ServiceLocator.Remove(this);
        }
        
        /// <summary>
        /// Gets a faction-wide service. To add a new service, add a child gameobject with an attached IFactionService MonoBehaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() where T : IWorldService => _services.OfType<T>().SingleOrDefault();
    }
}