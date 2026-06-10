using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sylpheed.UI
{
    public class ToggleWithText : MonoBehaviour
    {
        [SerializeField] private string _text;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private Toggle _toggle;

        public string Label
        {
            get => _labelText.text;
            set => _labelText.text = value;
        }
        
        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        public bool Interactable
        {
            get => _toggle.interactable;
            set => _toggle.interactable = value;
        }

        public Toggle.ToggleEvent OnValueChanged => _toggle.onValueChanged;
    }
}