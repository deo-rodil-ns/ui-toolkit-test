using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Player.Inputs.Entity;
using GothicVampire.Roads.Effects;
using Sylpheed.Core;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.Roads
{
    public class RoadView : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TMP_Text _streetNameText;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private Button _upgradeButton;

        [Header("Road")]
        [SerializeField] private TMP_Text _roadNameText;
        [SerializeField] private TMP_Text _roadDescriptionText;

        private RoadManager _roadManager;

        private RoadEffect _effect;
        private Road Road => _effect?.Road;
        private Wallet _wallet;
        private IEntitySelectorService _entitySelector;
        private int _selectedCount;

        public void Show(RoadEffect effect)
        {
            if (_entitySelector == null)
                _entitySelector = ServiceLocator.Get<IEntitySelectorService>();

            if (_roadManager == null)
                _roadManager = ServiceLocator.Get<World>().Player.GetService<RoadManager>();

            _effect = effect;
            _wallet = effect.Road.Faction.GetService<Wallet>();
            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            _entitySelector.EvtDoubleClickedUpdate.AddListener(Refresh);

            Refresh();

            _upgradeButton.gameObject.SetActive(Road.Data.Upgradable);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _wallet.EvtUpdated.RemoveListener(OnWalletUpdated);
            _entitySelector.EvtDoubleClickedUpdate.RemoveListener(Refresh);
            _selectedCount = 0;
            _effect = null;
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            _selectedCount = _entitySelector.GetSelectedEntities.Count(e => e.Type == EntityType.Road);

            _streetNameText.text = (_selectedCount > 1) ? $"{Road.DisplayName} x{_selectedCount}" : Road.DisplayName;
            _tierText.text = $"Tier {Road.Tier}";

            UpdateUpgradeButton();
        }

        private void OnWalletUpdated(Wallet wallet) => UpdateUpgradeButton();

        private void UpdateUpgradeButton()
        {
            if(_selectedCount == 1)
            {
                if (!Road.Data.Upgradable) return;
                _upgradeButton.interactable = Road.CanUpgrade;
            }
            else
            {
                //ToDo:
                //Re-visit how upgrading works for multiple entities.
            }
        }

        private void Sell()
        {
            if(_selectedCount == 1)
            {
                Road?.Sell();
            }
            else
            {
                //ToDo: Temporary, need to transfer this somewhere else, discuss.                
                foreach (SelectableEntity road in _entitySelector.GetSelectedEntities.ToList())
                {
                    var roadData = road.GetComponent<Road>();
                    roadData?.Sell();
                }
            }

            _roadManager.UpdateRoads();
        }

        public void Evt_SellPressed() => Sell();
    }
}
