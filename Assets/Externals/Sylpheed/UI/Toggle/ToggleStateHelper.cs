using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Sylpheed.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleStateHelper : MonoBehaviour
    {
        [Header("Additional Graphics")]
        [SerializeField] private List<GameObject> _toggledGraphics;
        [SerializeField] private List<GameObject> _untoggledGraphics;
        
        [Header("Target Graphic Color")] 
        [SerializeField] private bool _changeTargetGraphicColor;
        [SerializeField] private Color _toggledTargetGraphicColor;
        [SerializeField] private Color _unToggledTargetGraphicColor;
        [SerializeField] private Color _disabledTargetGraphicColor;

        [Header("Text")] 
        [SerializeField] private string _label;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Color _toggledFontColor;
        [SerializeField] private Color _untoggledFontColor;
        [SerializeField] private Color _disabledFontColor;

        [Space]
        [SerializeField] private UnityEvent _evtToggledOn;
        [SerializeField] private UnityEvent _evtToggledOff;
        [SerializeField] private UnityEvent _evtInteractionEnabled;
        [SerializeField] private UnityEvent _evtInteractionDisabled;
        [SerializeField] private UnityEvent<bool> _evtInteractableChanged;
        
        public Toggle Toggle { get; private set; }

        private bool? _prevInteractable;
        
        private void Awake()
        {
            Toggle = GetComponent<Toggle>();
        }

        private void Start()
        {
            OnToggleValueChanged(Toggle.isOn);
        }

        private void Update()
        {
            UpdateTargetGraphicColor();
            UpdateFontColor();
            CheckInteractable();
        }

        private void UpdateTargetGraphicColor()
        {
            if (!_changeTargetGraphicColor) return;
            if (Toggle.interactable) Toggle.targetGraphic.color = Toggle.isOn ? _toggledTargetGraphicColor : _unToggledTargetGraphicColor;
            else Toggle.targetGraphic.color = _disabledTargetGraphicColor;
        }

        private void CheckInteractable()
        {
            if (_prevInteractable != Toggle.interactable)
            {
                _evtInteractableChanged?.Invoke(Toggle.interactable);
                if (Toggle.interactable) _evtInteractionEnabled?.Invoke();
                else _evtInteractionDisabled?.Invoke();

                if (Toggle.isOn) Toggle.isOn = false;
            }
            
            _prevInteractable = Toggle.interactable;
        }

        private void UpdateFontColor()
        {
            if (!_text) return;
            
            if (Toggle.interactable)
            {
                _text.color = Toggle.isOn ? _toggledFontColor : _untoggledFontColor;
            }
            else
            {
                _text.color = _disabledFontColor;
            }
        }

        public void OnToggleValueChanged(bool isOn)
        {
            Toggle ??= GetComponent<Toggle>();
            
            _toggledGraphics.ForEach(g => g.SetActive(isOn));
            _untoggledGraphics.ForEach(g => g.SetActive(!isOn));
            
            UpdateTargetGraphicColor();
            UpdateFontColor();

            if (isOn)
                _evtToggledOn?.Invoke();
            else
                _evtToggledOff?.Invoke();
        }

        private void OnValidate()
        {
            if (_text) _text.text = _label;
        }
    }
}