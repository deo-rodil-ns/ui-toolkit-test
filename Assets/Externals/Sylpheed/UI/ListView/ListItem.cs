using System;
using UnityEngine;

namespace Sylpheed.UI
{
    public abstract class ListItem<T> : MonoBehaviour
    {
        public event Action<T> OnClicked;
        public event Action<T, bool> OnSelected;
        
        public T Data { get; private set; }

        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                _selected = value;
                OnSelectionChanged(_selected);
                OnSelected?.Invoke(Data, _selected);
            }
        }
        
        protected virtual void OnShow(T data) { }
        protected virtual void OnSelectionChanged(bool selected) { }
        protected virtual void OnPool() { }
        
        public void Show(T data, bool selected)
        {
            gameObject.SetActive(true);
            _selected = selected;
            Data = data;
            OnShow(data);
        }

        public void Refresh()
        {
            Show(Data, Selected);
        }

        public void Pool()
        {
            OnPool();
            gameObject.SetActive(false);
        }

        protected void RaiseClicked() => OnClicked?.Invoke(Data);
        protected void RaiseSelected(bool selected) => OnSelected?.Invoke(Data, selected);
    }
}