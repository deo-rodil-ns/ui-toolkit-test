using System;
using GothicVampire.Jobs;
using GothicVampire.Villagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class PopulationHudTierElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _villagerNameText;
        [SerializeField] private TMP_Text _populationText;

        private VillagerData _villagerData;
        private VillagerManager _villagerManager;
        private JobManager _jobManager;
        private bool _initialized;
        
        public void Initialize(VillagerData villagerData, VillagerManager villagerManager)
        {
            _villagerData = villagerData;
            _villagerManager = villagerManager;
            _jobManager = villagerManager.Faction.GetService<JobManager>();
            _initialized = true;
            
            Refresh();
            
            _villagerManager.EvtUpdated.AddListener(OnVillagersUpdated);
            _jobManager.EvtJobUpdated.AddListener(OnJobUpdated);
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            _villagerManager?.EvtUpdated.AddListener(OnVillagersUpdated);
            _jobManager?.EvtJobUpdated.AddListener(OnJobUpdated);
        }

        private void OnJobUpdated(Job job) => Refresh();
        private void OnVillagersUpdated() => Refresh();

        private void Refresh()
        {
            // Don't refresh if disabled
            if (!isActiveAndEnabled || !_initialized) return;
            
            _icon.sprite = _villagerData.Icon;
            _villagerNameText.text = _villagerData.DisplayName;
            _populationText.text = $"{_villagerManager.GetAssignedVillagers(_villagerData.Tier).Count} / {_villagerManager.GetVillagers(_villagerData.Tier).Count}";
        }
    }
}