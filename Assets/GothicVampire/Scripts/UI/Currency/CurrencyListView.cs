using System;
using System.Collections.Generic;
using GothicVampire.Currencies;
using UnityEngine;

namespace GothicVampire.UI.Currencies
{
    public class CurrencyListView : MonoBehaviour
    {
        [SerializeField] private CurrencyListElement _template;

        private readonly List<CurrencyListElement> _elements = new();

        private void Awake()
        {
            if (_template && _template.gameObject.scene.IsValid()) _template.gameObject.SetActive(false);
        }

        public void Show(IReadOnlyCollection<Currency> currencies)
        {
            // Create elements
            Clear();
            foreach (var currency in currencies)
            {
                var element = Instantiate(_template, _template.transform.parent);
                element.Show(currency);
                _elements.Add(element);
            }
            
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Show and have elements respond based on provided sufficiency checker.
        /// </summary>
        /// <param name="currencies"></param>
        /// <param name="sufficiencyChecker"></param>
        public void Show(IReadOnlyCollection<Currency> currencies, Func<Currency, bool> sufficiencyChecker)
        {
            // Create elements
            Clear();
            foreach (var currency in currencies)
            {
                var element = Instantiate(_template, _template.transform.parent);
                element.Show(currency, sufficiencyChecker);
                _elements.Add(element);
            }
            
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Clear();
        }

        private void Clear()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();
        }
    }
}