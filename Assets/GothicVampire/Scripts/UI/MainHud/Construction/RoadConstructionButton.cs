using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Grids;
using GothicVampire.Roads;
using Sylpheed.Core;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class RoadConstructionButton : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private RoadConstructionTooltip _tooltip;

        [Header("Color")]
        [SerializeField] private Image[] _themedImages;
        #endregion

        private RoadData _data;
        private RoadPlacementSystem _placementSystem;
        private RoadManager _buildingManager;
        private Wallet _wallet;

        private void OnEnable()
        {
            _tooltip.gameObject.SetActive(false);
        }

        public void Initialize(RoadData data, Faction faction)
        {
            _data = data;
            _placementSystem = ServiceLocator.Get<RoadPlacementSystem>();
            _buildingManager = faction.GetService<RoadManager>();
            _wallet = faction.GetService<Wallet>();
            
            _tooltip.Initialize(data, faction);
            
            _icon.sprite = data.ConstructionIcon;
            _themedImages.ForEach(i => i.color = _data.ConstructionCodeColor);

            _wallet.EvtUpdated.AddListener(OnWalletUpdated);

            Refresh();
        }
        
        private void Refresh()
        {
            _button.interactable = _buildingManager.CanPurchase(_data);
        }

        private void OnWalletUpdated(Wallet wallet) => Refresh();
        public void Evt_PurchasePressed() => _placementSystem.PlaceRoad(_data);
    }
}
