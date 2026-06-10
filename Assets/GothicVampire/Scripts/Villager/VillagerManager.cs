using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using GothicVampire.Game;
using GothicVampire.Productions;
using GothicVampire.Unrest;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Villagers
{
    public sealed class VillagerManager : MonoBehaviour, IFactionService, IUnrestPredictor
    {
        [SerializeField] private VillagerSettings _settings;
        [SerializeField] private Transform _container;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<Villager> _evtVillagerAdded;
        [SerializeField] private UnityEvent<Villager> _evtVillagerRemoved;
        [SerializeField] private UnityEvent _evtUpdated;
        [SerializeField] private UnityEvent<UpkeepResolvedEventArgs> _evtUpkeepResolved;
        
        public IReadOnlyCollection<Villager> Villagers => _villagers;
        public IReadOnlyCollection<Villager> AssignedVillagers => _villagers.Where(v => v.Job != null).ToArray();
        public IReadOnlyCollection<Villager> UnassignedVillagers => _villagers.Where(v => v.Job == null).ToArray();
        
        public IReadOnlyCollection<Currency> UpkeepCost { get; private set; } = new List<Currency>();
        public VillagerSettings Settings => _settings;
        
        public IReadOnlyCollection<Villager> GetVillagers(int tier) => _villagers.Where(v => v.Data.Tier == tier).ToArray();
        public IReadOnlyCollection<Villager> GetUnassignedVillagers(int tier) => _villagers.Where(v => v.Job == null & v.Data.Tier == tier).ToArray();
        public IReadOnlyCollection<Villager> GetAssignedVillagers(int tier) => _villagers.Where(v => v.Job != null & v.Data.Tier == tier).ToArray();

        #region Events

        public UnityEvent<Villager> EvtVillagerAdded => _evtVillagerAdded;
        public UnityEvent<Villager> EvtVillagerRemoved => _evtVillagerRemoved;
        public UnityEvent EvtUpdated => _evtUpdated;
        public UnityEvent<UpkeepResolvedEventArgs> EvtUpkeepResolved => _evtUpkeepResolved;
        
        public class UpkeepResolvedEventArgs
        {
            public IReadOnlyCollection<Currency> UpkeepPaid { get; internal set; }
            public IReadOnlyCollection<Currency> UpkeepUnpaid { get; internal set; }
        }

        #endregion
        
        private readonly List<Villager> _villagers = new();
        private Wallet _wallet;
        private UnrestActor _unrestActor;

        /// <summary>
        /// Create villager from VillagerData then initialize and track it.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Villager CreateVillager(VillagerData data, VillagerSource source)
        {
            // Initialize and track villager
            var villager = Instantiate(_settings.VillagerPrefab, _container);
            villager.Initialize(data, source);
            villager.gameObject.name = source?.Building 
                ? $"{villager.Data.DisplayName} Tier {villager.Data.Tier} ({source.Building.DisplayName})" 
                : $"{villager.Data.DisplayName} Tier {villager.Data.Tier}";
            AddVillager(villager);
            
            return villager;
        }

        /// <summary>
        /// Create villagers from VillagerData then initialize and track it.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IReadOnlyCollection<Villager> CreateVillagers(VillagerData data, VillagerSource source, int count = 1)
        {
            var villagers = new List<Villager>();
            for (var i = 0; i < count; i++)
            {
                var villager = CreateVillager(data, source);
                villagers.Add(villager);
            }
            
            return villagers;
        }
        
        /// <summary>
        /// Tracks an already created and initialized villager.
        /// </summary>
        /// <param name="villager"></param>
        public void AddVillager(Villager villager)
        {
            // Don't register already registered villagers
            if (_villagers.Any(v => v == villager)) return;
            _villagers.Add(villager);
            
            UpdateUpkeepCost();
            
            // Remove villager if it was destroyed
            villager.EvtDestroying.AddListener(v => RemoveVillager(v));
            
            EvtVillagerAdded?.Invoke(villager);
            EvtUpdated?.Invoke();
        }

        /// <summary>
        /// Remove the villager.
        /// </summary>
        /// <param name="villager"></param>
        /// <param name="destroy"></param>
        public void RemoveVillager(Villager villager, bool destroy = false)
        {
            if (_villagers.All(v => v != villager)) return;
            _villagers.Remove(villager);
            
            UpdateUpkeepCost();
            
            EvtVillagerRemoved?.Invoke(villager);
            EvtUpdated?.Invoke();
            
            if (destroy) Destroy(villager.gameObject);
        }

        /// <summary>
        /// Remove the villagers.
        /// </summary>
        /// <param name="villagers"></param>
        /// <param name="destroy"></param>
        public void RemoveVillagers(IEnumerable<Villager> villagers, bool destroy = false)
        {
            villagers.ForEach(v => RemoveVillager(v, destroy));
        }

        /// <summary>
        /// Pay upkeep cost based on villagers.
        /// Called internally by cycle tasks.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<Currency> ResolveUpkeepCost(CycleBehaviorSnapshot snapshot)
        {
            var upkeepPaid = new CurrencyCollection();
            var upkeepUnpaid = new CurrencyCollection();
            var villagerPaid = new List<Villager>();
            var villagerUnpaid = new List<Villager>();
            
            foreach (var villager in _villagers)
            {
                // Deduct upkeep cost if we can afford it
                var cost = villager.Data.UpkeepCost;
                
                // Check if we can pay for the upkeep cost
                if (!_wallet.HasEnough(cost))
                {
                    upkeepUnpaid.Add(cost);
                    villagerUnpaid.Add(villager);
                    
                    continue;
                }
                
                // Pay upkeep cost
                _wallet.Deduct(cost);
                upkeepPaid.Add(cost);
                villagerPaid.Add(villager);
            }
            
            // Unrest
            villagerPaid.ForEach(v => _unrestActor.AddUnrest(Settings.PaidUpkeepUnrest));
            villagerUnpaid.ForEach(v => _unrestActor.AddUnrest(Settings.UnpaidUpkeepUnrest));
            
            EvtUpkeepResolved.Invoke(new UpkeepResolvedEventArgs
            {
                UpkeepPaid = upkeepPaid,
                UpkeepUnpaid = upkeepUnpaid,
            });
            
            Debug.Log($"[VillagerManager] Paid: {upkeepPaid.FormatToString()}" +
                      $"\nUnpaid: {upkeepUnpaid.FormatToString()}");
            
            // Snapshot
            snapshot.CurrencyChanged.Deduct(upkeepPaid);
            
            return upkeepPaid;
        }

        private void UpdateUpkeepCost()
        {
            UpkeepCost = _villagers.SelectMany(v => v.Data.UpkeepCost).Collate().ToList();
        }

        #region IFactionService
        public Faction Faction { get; set; }
        void IFactionService.OnFactionInitialize(Faction faction)
        {
            _wallet = faction.GetService<Wallet>();
            _unrestActor = faction.GetService<UnrestActor>();
            
            // Create initial villagers
            if (_settings.NumInitialVillagers > 0) CreateVillagers(_settings.InitialVillager, null, _settings.NumInitialVillagers);
            
            // Register unrest predictor
            _unrestActor.AddPredictor(this);
        }

        #endregion

        void IUnrestPredictor.Predict(UnrestSnapshot snapshot)
        {
            // Predict wallet after a main cycle production
            var productionManager = Faction.GetService<ProductionManager>();
            var currencies = new CurrencyCollection(_wallet.Currencies, false);
            currencies.Add(productionManager.NetProjection);
            
            // Simulate upkeep cost
            foreach (var villager in _villagers)
            {
                // Deduct upkeep cost if we can afford it
                var cost = villager.Data.UpkeepCost;
                
                // Check if we can pay for the upkeep cost
                if (!currencies.HasEnough(cost))
                {
                    snapshot.AddSource(Settings.UnpaidUpkeepUnrest);
                    continue;
                }
                
                // Pay upkeep cost
                currencies.Deduct(cost);
                snapshot.AddSource(Settings.PaidUpkeepUnrest);
            }
        }
    }
}