using System;
using System.Collections.Generic;
using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Grids;
using GothicVampire.Technologies;
using GothicVampire.UI.Currencies;
using Sylpheed.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Buildings
{
    public class SimpleBuildingView : MonoBehaviour
    {
        [Header("General")] 
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _closeButton;
        
        [Header("Construction")] 
        [SerializeField] private Image _constructionProgressFill;
        [SerializeField] private TMP_Text _constructionProgressText;
        [SerializeField] private List<GameObject> _objectsToEnableDuringConstruction;
        [SerializeField] private List<GameObject>  _objectsToDisableDuringConstruction;

        [Header("Tooltip")]
        [SerializeField] private CurrencyListView _upgradeTooltip;
        [SerializeField] private CurrencyListView _sellTooltip;
        [SerializeField] private CurrencyListView _cancelTooltip;

        public Button CloseButton => _closeButton;
        
        private Building _building;
        private Wallet _wallet;
        private TechnologyManager _technologyManager;

        private void Start()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        private void OnEnable()
        {
            _upgradeTooltip.Hide();
            _sellTooltip.Hide();
            _cancelTooltip.Hide();
        }

        public void Show(Building building)
        {
            _building = building;
            _wallet = _building.Faction.GetService<Wallet>();
            _technologyManager = _building.Faction.GetService<TechnologyManager>();
            
            Refresh();
            
            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            _building.Construction.EvtStateUpdated?.AddListener(OnConstructionUpdated);
            _building.Construction.EvtProgressUpdated?.AddListener(OnConstructionProgressUpdated);
            _building.EvtTierUpdated.AddListener(OnTierUpdated);
            _technologyManager.EvtUpdated.AddListener(OnTechnologyUpdated);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _wallet?.EvtUpdated.RemoveListener(OnWalletUpdated);
            _building?.Construction.EvtStateUpdated?.RemoveListener(OnConstructionUpdated);
            _building?.Construction.EvtProgressUpdated?.RemoveListener(OnConstructionProgressUpdated);
            _building?.EvtTierUpdated.RemoveListener(OnTierUpdated);
            _technologyManager?.EvtUpdated.RemoveListener(OnTechnologyUpdated);
            _building = null;

            gameObject.SetActive(false);
        }
        
        private void Refresh()
        {
            _nameText.text = _building.DisplayName;
            _descriptionText.text = _building.Data.Description;
            _tierText.text = $"T{_building.CurrentTier?.TierLevel}";
            
            // Show/hide panels
            _objectsToEnableDuringConstruction.ForEach(o => o.SetActive(_building.Construction.InProgress));
            _objectsToDisableDuringConstruction.ForEach(o => o.SetActive(!_building.Construction.InProgress));
            
            UpdateUpgradeButton();
        }
        
        private void UpdateUpgradeButton()
        {
            _upgradeButton.gameObject.SetActive(_building.Data.Upgradable && _building.Construction.Ready);
            _upgradeButton.interactable = _building.CanUpgrade;
        }
        
        private void OnConstructionProgressUpdated(BuildingConstruction construction)
        {
            _constructionProgressFill.fillAmount = _building.Construction.Progress;
            _constructionProgressText.text = $"Time Left: {_building.Construction.TimeRemaining:N0}s";
        }

        private void OnConstructionUpdated(BuildingConstruction construction)
        {
            _upgradeTooltip.Hide();
            _sellTooltip.Hide();
            _cancelTooltip.Hide();
            Refresh();
        }
        private void OnWalletUpdated(Wallet wallet) => UpdateUpgradeButton();
        private void OnTierUpdated(Building building) => Refresh();
        private void OnTechnologyUpdated() => UpdateUpgradeButton();

        public void Evt_UpgradePressed()
        {
            _building?.Upgrade().Forget();
        }

        public void Evt_RelocatePressed()
        {
            var gridService = ServiceLocator.Get<IBuildingService>();
            gridService.RelocateBuilding();
        }
        
        public void Evt_SellPressed() => _building?.Sell();
        public void Evt_CancelPressed() => _building.Construction.Interrupt();

        public void Evt_UpgradeButtonHovered()
        {
            if (!_building.CanUpgrade) return;
            _upgradeTooltip.Show(_building.NextTier.BuildCost);
        }
        public void Evt_SellButtonHovered() => _sellTooltip.Show(_building.SellValue);
        public void Evt_CancelHovered() => _cancelTooltip.Show(_building.PendingCost);
    }
}