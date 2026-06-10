using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Currencies;
using GothicVampire.UI.Currencies;
using GothicVampire.Villagers;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.Buildings
{
    public class VillagerBuildingView : MonoBehaviour
    {
        [Header("Villager")] 
        [SerializeField] private TMP_Text _headerSummaryText;
        [SerializeField] private VillagerBuildingInfoVillagerElement _villagerElementTemplate;
        
        [Header("Subviews")]
        [SerializeField] private CurrencyListView _upkeepCostView;
        [SerializeField] private SimpleBuildingView _simpleBuildingView;

        public SimpleBuildingView SimpleBuildingView => _simpleBuildingView;
        
        private Building _building;
        private AddVillager Effect => _building?.Effect as AddVillager;
        private readonly List<VillagerBuildingInfoVillagerElement> _villagerElements = new();
        private VillagerManager _villagerManager;
        private IReadOnlyList<Villager> _villagers = new List<Villager>();

        private void Awake()
        {
            _villagerElementTemplate.gameObject.SetActive(false);
        }

        public void Show(Building building)
        {
            _building = building;
            _villagerManager = building.Faction.GetService<VillagerManager>();

            Refresh();
            _simpleBuildingView.Show(_building);
            
            _building.EvtTierUpdated?.AddListener(OnTierUpdated);
            
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            _building?.EvtTierUpdated?.RemoveListener(OnTierUpdated);
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            // Update cached villagers
            _villagers = _villagerManager.Villagers.Where(v => v.Source.Building == _building).ToList();

            // Update UI
            _headerSummaryText.text = $"{_villagers.Count} - Tier {_building.CurrentTier.TierLevel}";
            _upkeepCostView.Show(Effect?.UpkeepCost ?? Array.Empty<Currency>());
            CreateVillagerElements();
        }

        private void CreateVillagerElements()
        {
            _villagerElements.ForEach(e => Destroy(e.gameObject));
            _villagerElements.Clear();

            foreach (var villager in _villagers)
            {
                var element = Instantiate(_villagerElementTemplate, _villagerElementTemplate.transform.parent);
                _villagerElements.Add(element);
                element.Show(villager);
                element.gameObject.SetActive(true);
            }
        }

        private void OnTierUpdated(Building building) => Refresh();
    }
}