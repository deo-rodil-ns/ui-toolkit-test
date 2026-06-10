using System.Collections.Generic;
using GothicVampire.Buildings;
using GothicVampire.Game;

namespace GothicVampire.Villagers
{
    public sealed class VillagerSource
    {
        public Faction Faction { get; }
        public Building Building { get; }
        public IReadOnlyCollection<Villager> Villagers => _villagers;

        private readonly HashSet<Villager> _villagers = new();
        
        public VillagerSource(Faction faction, Building building = null)
        {
            Faction = faction;
            Building = building;
        }

        /// <summary>
        /// Called internally by Villager when it's initialized.
        /// </summary>
        /// <param name="villager"></param>
        public void AddVillager(Villager villager)
        {
            _villagers.Add(villager);
        }

        /// <summary>
        /// Called internally by Villager when it's destroyed.
        /// </summary>
        /// <param name="villager"></param>
        public void RemoveVillager(Villager villager)
        {
            _villagers.Remove(villager);
        }
    }
}