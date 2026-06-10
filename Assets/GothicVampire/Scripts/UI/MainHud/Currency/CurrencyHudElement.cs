using System;
using System.Globalization;
using GothicVampire.CodeBasedAnimators;
using GothicVampire.Currencies;
using GothicVampire.Productions;
using System.Linq;
using GothicVampire.Cycles;
using GothicVampire.Villagers;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class CurrencyHudElement : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _amountText;

        [Header("Net")]
        [SerializeField] private Image _netIcon;
        [SerializeField] private TMP_Text _netText;
        [SerializeField] private RectTransform _netTransform;
        [SerializeField] private GameObject _netContainer;
        [SerializeField] private float _netAnimationDuration = 1f;

        [Header("Tooltip")]
        [SerializeField] private TMP_Text _consumedText;
        [SerializeField] private TMP_Text _producedText;
        [SerializeField] private GameObject _tooltip;

        [Header("Configuration")]
        [SerializeField] private int _totalAmountDigits = 6;
        [SerializeField] private Color _positiveProductionColor;
        [SerializeField] private Color _negativeProductionColor;
        #endregion

        private Color _originalColor;
        private TextUIColorAnimator _netColorAnimator;
        private Wallet _wallet;
        private CurrencyType _currencyType;
        private FactionCycleManager _factionCycleManager;
        private VillagerManager _villagerManager;
        private ProductionManager _productionManager;
        private bool _projectionUpdateQueued;

        private void Start()
        {
            _netColorAnimator = new TextUIColorAnimator(_netText);
        }

        private void OnEnable()
        {
            _tooltip.SetActive(false);
            _netContainer.SetActive(false);
        }

        public void Initialize(CurrencyType type, Wallet wallet)
        {
            _currencyType = type;
            _wallet = wallet;
            _factionCycleManager = _wallet.Faction.GetService<FactionCycleManager>();
            _villagerManager = _wallet.Faction.GetService<VillagerManager>();
            _productionManager = _wallet.Faction.GetService<ProductionManager>();

            _originalColor = _netText.color;
            _icon.sprite = type.Icon;
            _nameText.text = type.DisplayName;

            _netIcon.sprite = type.Icon;
            _netText.text = string.Empty;
            _consumedText.text = "0";
            _producedText.text = "0";

            _wallet.EvtUpdated?.AddListener(OnWalletUpdated);
            _factionCycleManager.EvtCycleResolved?.AddListener(OnCycleResolved);
            _productionManager.EvtProjectionUpdated?.AddListener(OnProductionProjectionUpdated);
            _villagerManager.EvtUpdated?.AddListener(OnVillagerManagerUpdated);

            Refresh();
        }
        
        private void OnDestroy()
        {
            _wallet?.EvtUpdated?.RemoveListener(OnWalletUpdated);
            _factionCycleManager?.EvtCycleResolved?.RemoveListener(OnCycleResolved);
        }

        private void OnCycleResolved(CycleBehaviorSnapshot snapshot)
        {
            // Net
            var net = snapshot.CurrencyChanged.Get(_currencyType) ?? default;
            if (net.IsValid && !Mathf.Approximately(net.Value, 0f)) ShowNetProduced(net);
        }

        private void OnWalletUpdated(Wallet wallet) => Refresh();
        
        private void OnVillagerManagerUpdated() => _projectionUpdateQueued = true;
        private void OnProductionProjectionUpdated(ProductionManager arg0) => _projectionUpdateQueued = true;

        private void Refresh()
        {
            var currency = _wallet.Get(_currencyType) ?? default;
            var rawAmount = currency.Value.ToString(CultureInfo.InvariantCulture);

            if (rawAmount.Length > _totalAmountDigits)
            {
                _amountText.text = "999999";
            }
            else if (rawAmount.Length == _totalAmountDigits)
            {
                _amountText.text = rawAmount;
            }
            else
            {
                string leadingDigits = string.Empty;

                for (int i = 0; i < _totalAmountDigits - rawAmount.Length; i++)
                {
                    leadingDigits += "0";
                }

                _amountText.text = $"<color=white>{leadingDigits}</color>{rawAmount}";
            }
        }

        private void UpdateProjection()
        {
            // Tooltip
            var consumed = _productionManager.InputProjection
                .Concat(_villagerManager.UpkeepCost)
                .Collate()
                .SingleOrDefault(c => c.Type == _currencyType);
            var produced = _productionManager.OutputProjection
                .SingleOrDefault(c => c.Type == _currencyType);
            _consumedText.text = (consumed.Value * -1f).ToStringWithPrefix("N0");
            _producedText.text = produced.Value.ToStringWithPrefix("N0");
        }

        private void ShowNetProduced(Currency currency)
        {
            if (!currency.IsValid) return;

            _netText.text = currency.Value > 0
                ? $"+{currency.Value:N0}"
                : $"{currency.Value:N0}";
            var textColor = currency.Value >= 0
                ? _positiveProductionColor
                : _negativeProductionColor;
            _netText.color = textColor;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_netTransform);

            // Animate
            _netColorAnimator.Stop();
            _netContainer.SetActive(true);
            _netColorAnimator.Start(textColor, _originalColor, _netAnimationDuration,
                onDone: () => _netContainer.SetActive(false),
                onStopped: () => _netContainer.SetActive(false)
                );
        }

        private void LateUpdate()
        {
            if (_projectionUpdateQueued)
            {
                _projectionUpdateQueued = false;
                UpdateProjection();
            }
        }
    }
}
