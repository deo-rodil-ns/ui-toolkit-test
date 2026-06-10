using System;
using GothicVampire.Abilities;
using GothicVampire.UI.Currencies;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.Abilities
{
    public class AbilityButtomTooltip : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _cooldownText;
        [SerializeField] private CurrencyListView _costView;
        private AbilityData _abilityData;
        private bool _isShown;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Initialize(AbilityData abilityData)
        {
            _abilityData = abilityData;

            _nameText.text = _abilityData.DisplayName;
            //_descriptionText.text = _abilityData.Description;
            _cooldownText.text = _abilityData.Cooldown.Duration.ToString();
            //TODO: Adjust CostView later on to be able to read ability requirements OR Create a new Costview for this.
            //_costView
        }
        
        public void Show()
        {
            if (_isShown) return;
            _isShown = true;
            
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (!_isShown) return;
            _isShown = false;
            
            gameObject.SetActive(false);
        }
    }
}
