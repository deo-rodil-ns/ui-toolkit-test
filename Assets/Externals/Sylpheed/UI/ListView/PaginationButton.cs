using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sylpheed.UI
{
    public enum PaginationButtonType { Index, First, Last, Next, Previous, Truncated };

    public class PaginationButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Toggle _toggle;

        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;
        
        public UnityEvent<PaginationButton> Clicked;

        public int Index { get; private set; }
        public PaginationButtonType Type { get; private set; }

        private bool _initialized;

        private void OnDisable()
        {
            _initialized = false;
        }

        public void Init(int index, PaginationButtonType type, bool selected)
        {
            _initialized = true;
            Index = index;
            Type = type;

            _text.text = type switch
            {
                PaginationButtonType.Index => $"{index + 1}",
                PaginationButtonType.First => "1",
                PaginationButtonType.Last => $"{index + 1}",
                PaginationButtonType.Previous => "<",
                PaginationButtonType.Next => ">",
                PaginationButtonType.Truncated => "...",
                _ => _text.text
            };

            gameObject.name = _text.text;
            _text.color = selected ? _selectedColor : _unselectedColor;
            _toggle.SetIsOnWithoutNotify(selected);
        }
        public void OnToggleValueChanged(bool toggled)
        {
            _text.color = toggled ? _selectedColor : _unselectedColor;

            // NOTE: This prevents Selected event to be called when this object is created
            if (_initialized && toggled) Clicked?.Invoke(this);
        }
    }
}

