using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Roads;
using GothicVampire.UI.Currencies;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class RoadConstructionTooltip : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _loreText;
        [SerializeField] private GameObject _insufficientCostPanel;
        [SerializeField] private CurrencyListView _costView;
        
        [Header("Stats")]
        [SerializeField] private GameObject _effectsContainer;
        [SerializeField] private TMP_Text _effectTemplate;

        [Header("Color")] 
        [SerializeField] private Image[] _themedImages;
        [SerializeField] private Image _gradientImage;
        [SerializeField] private Color _gradientAvailableColor;
        [SerializeField] private Color _gradientUnavailableColor;

        private RoadData _data;
        private Wallet _wallet;
        private readonly List<TMP_Text> _effectElements = new();
        private bool _initialized;
        private bool _isShown;
        
        private void OnEnable()
        {
            _effectTemplate.gameObject.SetActive(false);
        }

        public void Initialize(RoadData data, Faction faction)
        {
            if (_initialized) return;
            _initialized = true;
            
            _data = data;
            _wallet = faction.GetService<Wallet>();
        }

        public void Show()
        {
            if (_isShown) return;
            _isShown = true;
            
            gameObject.SetActive(true);
            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            
            Refresh();
        }

        public void Hide()
        {
            if (!_isShown) return;
            _isShown = false;
            
            gameObject.SetActive(false);
            _wallet.EvtUpdated.RemoveListener(OnWalletUpdated);
        }

        private void Refresh()
        {
            _nameText.text = _data.DisplayName;
            _descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_data.Description)); // Adjust layout when there's no description
            _descriptionText.text = _data.Description;
            _icon.sprite = _data.InfoIcon;
            _loreText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_data.Lore));
            _loreText.text = _data.Lore;
            _costView.Show(_data.BuildCost, currency => _wallet.HasEnough(currency));
            
            // TODO: Can only support Image for now but this can be used for gradient later (add a new reference to the UI component)
            _themedImages.ForEach(i => i.color = _data.ConstructionCodeColor);

            CreateEffectDescription();
            UpdateInsufficientCostNotification();
        }
        
        private void CreateEffectDescription()
        {
            // Clear previous
            _effectElements.ForEach(e => Destroy(e.gameObject));
            _effectElements.Clear();
            
            var descriptions = _data.EffectDescription;
            
            // Show/hide stats
            _effectsContainer.SetActive(descriptions.Any());
            
            // Create elements
            foreach (var description in descriptions)
            {
                var element = Instantiate(_effectTemplate, _effectTemplate.transform.parent);
                element.text = description;
                element.gameObject.SetActive(true);
                _effectElements.Add(element);
            }
        }

        private void UpdateInsufficientCostNotification()
        {
            bool hasEnough = _wallet.HasEnough(_data.BuildCost);
            _insufficientCostPanel.SetActive(!hasEnough);
            _gradientImage.color = hasEnough ? _gradientAvailableColor : _gradientUnavailableColor;
        }

        private void OnWalletUpdated(Wallet wallet) => UpdateInsufficientCostNotification();
    }
}