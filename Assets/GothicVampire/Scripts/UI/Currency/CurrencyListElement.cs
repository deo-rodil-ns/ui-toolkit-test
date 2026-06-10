using System;
using GothicVampire.Currencies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Currencies
{
    public class CurrencyListElement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _background;

        [Header("Value")] 
        [SerializeField] private ValueType _valueType;
        [SerializeField] private string _valueFormat = "N0";
        [SerializeField] private bool _showMaxValue;
        [SerializeField] private Image _currentMaxFill;
        
        [Header("Sufficiency State")]
        [SerializeField] private Color _sufficientColor;
        [SerializeField] private Color _insufficientColor;

        private Color? _originalColor;

        private void Awake()
        {
            _originalColor = _valueText?.color ?? default;
        }

        public void Show(Currency currency)
        {
            _icon.sprite = currency.Type.Icon;
            if (_valueText) _valueText.text = FormatCurrencyValue(currency);
            if (_valueText) _valueText.color = _originalColor ?? _valueText.color;
            if (_nameText) _nameText.text = currency.Type.DisplayName;
            if (_background) _background.color = currency.Type.BackgroundColor;
            
            // Current/Max fill
            if (_currentMaxFill)
            {
                _currentMaxFill.fillAmount = currency.HasMax 
                    ? Mathf.Clamp01(currency.Value / currency.Max) 
                    : 1f;
            }
            
            gameObject.SetActive(true);
        }

        // Change value text color based on sufficiency
        public void Show(Currency currency, Func<Currency, bool> sufficiencyChecker)
        {
            Show(currency);
            if (_valueText) _valueText.color = sufficiencyChecker(currency) ? _sufficientColor : _insufficientColor;
        }

        private string FormatCurrencyValue(Currency currency)
        {
            return _valueType switch
            {
                ValueType.Delta => FormatValueDelta(currency),
                ValueType.Expense => FormatValueExpense(currency),
                _ => FormatValueDefault(currency)
            };
        }

        private string FormatMaxValue(Currency currency)
        {
            if (_showMaxValue) return string.Empty;
            if (!currency.HasMax) return string.Empty;
            
            return $"/{currency.Max.ToString(_valueFormat)}";
        }

        private string FormatValueDefault(Currency currency)
        {
            return $"{currency.Value.ToString(_valueFormat)}{FormatMaxValue(currency)}";
        }

        private string FormatValueDelta(Currency currency)
        {
            if (Mathf.Approximately(currency.Value, 0)) return currency.Value.ToString(_valueFormat);
            
            var sign = currency.Value > 0 ? "+" : "-";
            return $"{sign}{currency.Value.ToString(_valueFormat)}{FormatMaxValue(currency)}";
        }

        private string FormatValueExpense(Currency currency)
        {
            return $"-{Mathf.Abs(currency.Value).ToString(_valueFormat)}{FormatMaxValue(currency)}";
        }
    }
    
    internal enum ValueType
    {
        Default, // Value is displayed as-is. Positive values with no positive prefix. Negative values with negative prefix.
        Delta, // Value is displayed as delta or "change". Positive values with positive prefix. Negative values with negative prefix. 
        Expense // Value is displayed as "expense". Will be displayed as absolute value (ignores positivity/negativity) prefixed with negative sign.
    }
}