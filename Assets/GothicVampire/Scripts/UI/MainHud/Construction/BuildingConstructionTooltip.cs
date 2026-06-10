using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Technologies;
using GothicVampire.UI.Currencies;
using GothicVampire.UI.Technologies;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class BuildingConstructionTooltip : MonoBehaviour
    {
        [Header("Description")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _buildingIcon;
        [SerializeField] private TMP_Text _loreText;
        [SerializeField] private TMP_Text _buildTimeText;
        [SerializeField] private GameObject _insufficientCostPanel;
        [SerializeField] private CurrencyListView _costView;

        [Header("Locked")] 
        [SerializeField] private PrerequisiteListView _requirementListView;
        [SerializeField] private GameObject _howToUnlockedPanel;
        
        [Header("Stats")]
        [SerializeField] private GameObject _effectsContainer;
        [SerializeField] private TMP_Text _effectTemplate;

        [Header("Color")]
        [SerializeField] private Image[] _themedImages;
        [SerializeField] private Image _gradientImage;
        [SerializeField] private Color _gradientAvailableColor;
        [SerializeField] private Color _gradientUnavailableColor;
        
        private BuildingData _data;
        private Wallet _wallet;
        private readonly List<TMP_Text> _effectElements = new();
        private bool _initialized;
        private bool _isShown;
        
        private TechnologyManager _technologyManager;
        private BuildingManager _buildingManager;
        
        private void OnEnable()
        {
            _effectTemplate.gameObject.SetActive(false);
        }

        public void Initialize(BuildingData data, Faction faction)
        {
            if (_initialized) return;
            _initialized = true;
            
            _data = data;
            _wallet = faction.GetService<Wallet>();
            _technologyManager = faction.GetService<TechnologyManager>();
            _buildingManager = faction.GetService<BuildingManager>();
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
            _buildingIcon.sprite = _data.InfoIcon;
            _loreText.gameObject.SetActive(!string.IsNullOrWhiteSpace(_data.Lore));
            _loreText.text = _data.Lore;
            _buildTimeText.text = $"{_data.BuildTime:N0}s";
            _costView.Show(_data.BuildCost, currency => _wallet.HasEnough(currency));
            
            // TODO: Can only support Image for now but this can be used for gradient later (add a new reference to the UI component)
            _themedImages.ForEach(i => i.color = _data.ConstructionCodeColor);
            
            CreateEffectDescription();
            UpdateInsufficientCostNotification();
            UpdateHowToUnlockNotification();
        }

        private void CreateEffectDescription()
        {
            // Clear previous
            _effectElements.ForEach(e => Destroy(e.gameObject));
            _effectElements.Clear();
            
            var effects = _data.EffectDescription;
            
            // Show/hide stats
            _effectsContainer.SetActive(effects.Any());
            
            // Create elements
            foreach (var effect in effects)
            {
                var element = Instantiate(_effectTemplate, _effectTemplate.transform.parent);
                element.text = effect;
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

        private void UpdateHowToUnlockNotification()
        {
            var tier = _data.Tiers.FirstOrDefault() ?? throw new Exception($"{_data.name} has no tier.");

            var prerequisite = _buildingManager.GetPrerequisite(tier);
            
            _requirementListView.Show(prerequisite);
            
            _howToUnlockedPanel.SetActive(!_technologyManager.IsUnlocked(tier));
        }
        
        private void OnWalletUpdated(Wallet wallet) => UpdateInsufficientCostNotification();
    }
}