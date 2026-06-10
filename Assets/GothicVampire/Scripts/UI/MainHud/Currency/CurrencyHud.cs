using GothicVampire.Currencies;
using GothicVampire.Game;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class CurrencyHud : MonoBehaviour
    {
        #region Inspector

        [Header("Configuration")]
        [SerializeField] private CurrencyType[] _leftCurrencyTypes;
        [SerializeField] private CurrencyType[] _rightCurrencyTypes;

        [Header("References")]
        [SerializeField] private CurrencyHudElement[] _leftCurrencyElements;
        [SerializeField] private CurrencyHudElement[] _rightCurrencyElements;
        [SerializeField] private BloodHud _bloodHud;
        #endregion

        private Wallet _wallet;
        private readonly List<CurrencyHudElement> _elements = new List<CurrencyHudElement>();

        private void Start()
        {
            _wallet = ServiceLocator.Get<World>().Player.GetService<Wallet>();

            InitializeElements();
            _bloodHud.Initialize(_wallet);
        }

        private void InitializeElements()
        {
            // Clear previous
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            // Left currencies
            for (int i = 0; i < _leftCurrencyTypes.Length; i++)
            {
                CurrencyType type = _leftCurrencyTypes[i];
                CurrencyHudElement element = _leftCurrencyElements[i];
                element.name = $"Element - {type.DisplayName}";
                element.gameObject.SetActive(true);
                _elements.Add(element);

                element.Initialize(type, _wallet);
            }

            // Right currencies
            for (int i = 0; i < _rightCurrencyTypes.Length; i++)
            {
                CurrencyType type = _rightCurrencyTypes[i];
                CurrencyHudElement element = _rightCurrencyElements[i];
                element.name = $"Element - {type.DisplayName}";
                element.gameObject.SetActive(true);
                _elements.Add(element);

                element.Initialize(type, _wallet);
            }
        }
    }
}
