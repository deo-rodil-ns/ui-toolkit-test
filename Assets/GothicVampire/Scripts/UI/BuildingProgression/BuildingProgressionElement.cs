using System;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Game;
using GothicVampire.Technologies;
using GothicVampire.UI.Technologies;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.BuildingProgression
{
    public class BuildingProgressionElement : MonoBehaviour
    {
        [Header("Config")] 
        [SerializeField] private Sprite[] _iconBgTiers;
        
        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _iconBackground;
        [SerializeField] private TMP_Text _buildingNameText;
        [SerializeField] private TMP_Text _buildingTierText;
        [SerializeField] private PrerequisiteListView _prerequisiteListView;
        [SerializeField] private UnlockableBuildingListView _unlockListView;

        [Header("Maxed")] 
        [SerializeField] private GameObject _maxedPanel;
        [SerializeField] private GameObject _pendingPanel;
        [SerializeField] private GameObject[] _maxedOverlays;
        
        public BuildingTier Tier { get; private set; }
        
        private TechnologyManager _technologyManager;
        private BuildingManager _buildingManager;
        private Faction _faction;

        public void Show(BuildingTier tier, Faction faction)
        {
            Tier = tier;
            _faction = faction;
            _buildingManager = faction.GetService<BuildingManager>();
            _technologyManager = faction.GetService<TechnologyManager>();

            Refresh();
        }

        private void Refresh()
        {
            _icon.sprite = Tier.Building.InfoIcon;
            _iconBackground.sprite = _iconBgTiers.ElementAtOrDefault(Tier.TierLevel - 1) 
                                     ?? _iconBgTiers.LastOrDefault();
            _buildingNameText.text = Tier.DisplayName;
            _buildingTierText.text = $"Tier {Tier.TierLevel}";
            
            // Check if last tier is already unlocked. Show maxed overlay if already maxed
            var maxed = Tier.IsLast && _technologyManager.IsUnlocked(Tier);
            _maxedPanel.SetActive(maxed);
            _pendingPanel.SetActive(!maxed);
            _maxedOverlays.ForEach(overlay => overlay.SetActive(maxed));
            
            // Show prerequisites if not yet maxed
            if (!maxed) RefreshPrerequisites();
            
            // Unlockables
            _unlockListView.Show(Tier, _faction);
        }

        private void RefreshPrerequisites()
        {
            var prerequisites = _buildingManager.GetPrerequisite(Tier);
            _prerequisiteListView.Show(prerequisites);
        }
    }
}