using GothicVampire.CodeBasedAnimators;
using GothicVampire.Currencies;
using System;
using System.Linq;
using GothicVampire.Cycles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class BloodHud : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CurrencyType _currencyType;
        [SerializeField] private float _maxFillValue;

        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _bloodGuageFillImage;
        [SerializeField] private Image _shineFillImage;
        [SerializeField] private TMP_Text _valueText;

        [Header("Net")]
        [SerializeField] private Image _netIcon;
        [SerializeField] private TMP_Text _netText;
        [SerializeField] private RectTransform _netTransform;
        [SerializeField] private GameObject _netContainer;
        [SerializeField] private float _netAnimationDuration = 1f;

        [Header("Configuration")]
        [SerializeField] private Color _positiveProductionColor;
        [SerializeField] private Color _negativeProductionColor;

        private Color _originalColor;
        private TextUIColorAnimator _netColorAnimator;
        private Wallet _wallet;
        private FactionCycleManager _factionCycleManager;
        private Currency _previousCurrency;

        private void Start()
        {
            _netColorAnimator = new TextUIColorAnimator(_netText);
        }

        private void OnEnable()
        {
            _netContainer.SetActive(false);
        }

        public void Initialize(Wallet wallet)
        {
            _wallet = wallet;
            _factionCycleManager = _wallet.Faction.GetService<FactionCycleManager>();

            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            _factionCycleManager.EvtCycleResolved?.AddListener(OnCycleResolved);

            Refresh();
        }

        private void OnDestroy()
        {
            _wallet?.EvtUpdated.RemoveListener(OnWalletUpdated);
            _factionCycleManager?.EvtCycleResolved?.RemoveListener(OnCycleResolved);
        }

        private void Refresh()
        {
            var currency = _wallet.Get(_currencyType) ?? default;
            
            _valueText.text = currency.Value.ToString("N0");
            _fillImage.fillAmount = Mathf.Clamp01(currency.Value / _maxFillValue);
            _bloodGuageFillImage.fillAmount = Mathf.Clamp01(currency.Value / _maxFillValue);
            _shineFillImage.fillAmount = Mathf.Clamp01(currency.Value / _maxFillValue);

            _previousCurrency = currency;
        }
        
        private void OnWalletUpdated(Wallet wallet)
        {
            var currency = _wallet.Get(_currencyType) ?? default;
            if (!currency.IsValid) return;
            if (Mathf.Approximately(currency.Value, _previousCurrency.Value)) return;

            Refresh();
        }
        
        private void OnCycleResolved(CycleBehaviorSnapshot snapshot)
        {
            // Net
            var net = snapshot.CurrencyChanged.Get(_currencyType) ?? default;
            if (net.IsValid) ShowNetProduced(net);
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
    }
}