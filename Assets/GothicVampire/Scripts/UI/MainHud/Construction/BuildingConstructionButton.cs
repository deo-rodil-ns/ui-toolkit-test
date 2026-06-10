using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using Sylpheed.Core;
using Sylpheed.Extensions;
using System;
using GothicVampire.Technologies;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class BuildingConstructionButton : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private BuildingConstructionTooltip _tooltip;

        [Header("Color")]
        [SerializeField] private Image[] _themedImages;
        #endregion

        private BuildingData _data;
        private BuildingPlacementSystem _buildingPlacementSystem;
        private BuildingManager _buildingManager;
        private Wallet _wallet;
        private TechnologyManager _technologyManager;

        private void OnEnable()
        {
            _tooltip.gameObject.SetActive(false);
        }

        public void Initialize(BuildingData data, Faction faction)
        {
            _data = data;
            _buildingPlacementSystem = ServiceLocator.Get<BuildingPlacementSystem>();
            _buildingManager = faction.GetService<BuildingManager>();
            _wallet = faction.GetService<Wallet>();
            _technologyManager = faction.GetService<TechnologyManager>();
            
            _tooltip.Initialize(data, faction);
            
            _icon.sprite = data.ConstructionIcon;
            _themedImages.ForEach(i => i.color = _data.ConstructionCodeColor);

            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            _technologyManager.EvtUpdated.AddListener(OnTechnologyUpdated);

            Refresh();
        }

        private void OnDestroy()
        {
            _wallet?.EvtUpdated.RemoveListener(OnWalletUpdated);
            _technologyManager?.EvtUpdated.AddListener(OnTechnologyUpdated);
        }

        private void Refresh()
        {
            _button.interactable = _buildingManager.CanPurchase(_data);
        }

        private void OnWalletUpdated(Wallet wallet) => Refresh();
        private void OnTechnologyUpdated() => Refresh();

        public void Evt_PurchasePressed()
        {
            if (!_buildingManager.CanPurchase(_data)) return;
            _buildingPlacementSystem.PlaceBuilding(_data);
        }
    }
}
