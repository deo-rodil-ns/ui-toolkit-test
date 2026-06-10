using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Villagers;
using UnityEngine;

namespace GothicVampire.Buildings.Effects
{
    [System.Serializable]
    public sealed class AddVillager : BuildingEffect
    {
        [SerializeField] private VillagerData _villagerData;
        [SerializeField] private int _numVillagers;
        
        public VillagerData VillagerData => _villagerData;
        public int NumVillagers => _numVillagers;
        public IReadOnlyCollection<Currency> UpkeepCost { get; private set; } = new List<Currency>();
        
        private VillagerManager _villagerManager;
        private List<Villager> _villagers = new();
        
        public override IReadOnlyList<string> DescriptionList 
        {
            get
            {
                var descriptions = new List<string>();
                descriptions.Add($"+{_numVillagers} {_villagerData.DisplayName}");
                
                return descriptions;
            }
        }
        
        protected override void OnActivate(Building building)
        {
            if (building.VillagerSource == null) throw new Exception($"Building {building.DisplayName} does not have a VillagerSource");
            
            _villagerManager = building.Faction.GetService<VillagerManager>();
            
            // Get existing villagers that were created from the same building
            var existingVillagers = building.VillagerSource.Villagers.ToList();
            var numLacking = Math.Max(0, NumVillagers - existingVillagers.Count);
            var numExcess = Math.Max(0, existingVillagers.Count - NumVillagers);
            
            // Add more villagers if lacking
            if (numLacking > 0)
            {
                var added = _villagerManager.CreateVillagers(_villagerData, building.VillagerSource, numLacking);
                existingVillagers.AddRange(added);
            }
            
            // Remove excess villagers
            if (numExcess > 0)
            {
                var toRemove = existingVillagers.Take(numExcess).ToList();
                toRemove.ForEach(villager => existingVillagers.Remove(villager));
                _villagerManager.RemoveVillagers(toRemove);
            }
            
            // Update data of villagers
            existingVillagers.ForEach(villager => villager.UpdateData(_villagerData));
            _villagers = existingVillagers;
            
            UpdateUpkeepCost();
        }

        protected override void OnBuildingRemoved(Building building)
        {
            _villagerManager?.RemoveVillagers(_villagers, true);
            
            UpdateUpkeepCost();
        }

        private void UpdateUpkeepCost()
        {
            UpkeepCost = _villagers.SelectMany(v => v.Data.UpkeepCost).Collate();
        }
    }
}