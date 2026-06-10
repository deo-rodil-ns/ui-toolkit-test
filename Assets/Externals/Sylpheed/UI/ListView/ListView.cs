using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sylpheed.UI
{
    public enum PageLocation { First, Current, Last }
    
    public abstract class ListView<T> : MonoBehaviour
    {
        public event Action<T> OnClicked;
        public event Action<T, bool> OnSelected;
        public event Action<IReadOnlyCollection<T>> OnFilterUpdated;

        [Header("List Item")]
        [SerializeField] private ListItem<T> _itemPrefab;
        [SerializeField] private RectTransform _itemContainer;
        
        [Header("Pagination")] 
        [SerializeField] private bool _paginationEnabled = true;
        [SerializeField] private int _itemsPerPage = 5;
        [SerializeField] private int _maxVisiblePageButtons = 5;
        [SerializeField] private bool _showPreviousNextPageButtons = true;
        [SerializeField] private bool _showPageTruncation = true;
        [SerializeField] private bool _hidePageButtonsForSinglePage = true;
        
        [Header("Page Controls")]
        [SerializeField] private PaginationButton _pageButtonTemplate;
        [SerializeField] private RectTransform _pageButtonContainer;
        [SerializeField] private PaginationButton _customPreviousButton;
        [SerializeField] private PaginationButton _customNextButton;
        [SerializeField] private TMP_InputField _itemsPerPageInput;

        private List<T> _source = new();
        public IReadOnlyList<T> Source
        {
            get => _source;
            set
            {
                _source = value.ToList();

                // Initially include all data in the filter
                _filteredData = value.ToList();
                
                // Preserve selected data that are still in the list
                _selectedItems.RemoveAll(data => !_source.Any(d => IsDataEqual(d, data)));
                
                OnSourceUpdated(_source);
                
                Redraw();
            }
        }

        private readonly List<ListItem<T>> _items = new();
        public IReadOnlyList<ListItem<T>> Items => _items;
        
        public RectTransform Container => _itemContainer;

        private int _pageIndex;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                _pageIndex = value;

                // Clamp
                _pageIndex = Math.Min(_pageIndex, NumPages - 1);
                _pageIndex = Math.Max(_pageIndex, 0);

                _shouldDrawPage = true;
            }
        }
        public int NumPages { get; private set; } = 1;

        private List<T> _selectedItems = new();
        public IReadOnlyCollection<T> SelectedItems => _selectedItems;
        
        private List<T> _filteredData = new();
        private readonly List<PaginationButton> _pageButtons = new();
        private readonly Stack<PaginationButton> _pooledPageButtons = new();
        private readonly Stack<ListItem<T>> _pooledItems = new();
        private bool _shouldDrawPage;
        private bool _shouldRefreshCurrentItems;
        
        protected virtual void OnInitialized() { }
        protected virtual void OnSourceUpdated(IReadOnlyList<T> source) { }
        protected virtual void OnDataClicked(T data) { }
        protected virtual void OnDataSelected(T data, bool selected) { }
        protected virtual void OnDataShown(IReadOnlyList<T> data) { }
        protected virtual void OnListItemCreated(ListItem<T> item) { }
        protected virtual void OnListItemsCreated(IReadOnlyList<ListItem<T>> items) { }
        protected virtual void OnClear() { }
        protected virtual bool IsDataEqual(T d1, T d2) { return Equals(d1, d2); }
        
        public void Redraw()
        {
            NumPages = _paginationEnabled
                ? Math.Max((int)Mathf.Ceil((float)_filteredData.Count / _itemsPerPage), 1)
                : 1;
            PageIndex = 0;
        }

        public void Clear()
        {
            // Clear data
            Source = new List<T>();
            
            // Delete items
            foreach (var item in _items)
            {
                item.Pool();
                _pooledItems.Push(item);
            }
            _items.Clear();
            
            // Clear page buttons
            foreach (var pageButton in _pageButtons)
            {
                pageButton.gameObject.SetActive(false);
                _pooledPageButtons.Push(pageButton);
            }
            _pageButtons.Clear();
            _customNextButton?.gameObject.SetActive(false);
            _customPreviousButton?.gameObject.SetActive(false);
            
            OnClear();
        }

        /// <summary>
        /// Updates matching data instances. This will only change the data of each item, but not the contents of the list.
        /// </summary>
        /// <param name="data"></param>
        public void UpdateData(IReadOnlyCollection<T> data)
        {
            // Replace instances in list
            foreach (var obj in data)
            {
                // Data
                var existingData = _source.FirstOrDefault(d => IsDataEqual(d, obj));
                if (existingData != null) _source[_source.IndexOf(existingData)] = obj;;

                // Filter
                var existingFilter = _filteredData.FirstOrDefault(d => IsDataEqual(d, obj));
                if (existingFilter != null) _filteredData[_filteredData.IndexOf(existingFilter)] = obj;
                
                // Selection
                var existingSelection = _selectedItems.FirstOrDefault(d => IsDataEqual(d, obj));
                if (existingSelection != null) _selectedItems[_selectedItems.IndexOf(existingSelection)] = obj;;
            }
            
            // Refresh items
            foreach (var item in _items.Where(i => i.isActiveAndEnabled))
            {
                var updatedData = data.FirstOrDefault(d => IsDataEqual(d, item.Data));
                if (updatedData == null) continue;
                item.Show(updatedData, item.Selected);
            }
        }

        /// <summary>
        /// Updates matching data instance. This will only change the content of the data item, but not the contents of the list.
        /// </summary>
        /// <param name="data"></param>
        public void UpdateData(T data)
        {
            UpdateData(new [] { data });
        }

        public void RefreshCurrentItems()
        {
            _shouldRefreshCurrentItems = true;
        }
        
        private async UniTaskVoid RefreshCurrentItemsPoll()
        {
            while (!ReferenceEquals(this, null))
            {
                if (!_shouldRefreshCurrentItems)
                {
                    await UniTask.Yield();
                    continue;
                }
                _shouldRefreshCurrentItems = false;
                
                foreach (var item in _items.Where(i => i.isActiveAndEnabled))
                {
                    item.Show(item.Data, _selectedItems.Any(i => IsDataEqual(i, item.Data)));
                }
            }
        }

        public void Filter(IEnumerable<T> items)
        {
            _filteredData = items.Where(data => _source.Any(d => IsDataEqual(d, data))).ToList();
            OnFilterUpdated?.Invoke(_filteredData);
            Redraw();
        }

        public void ClearFilter()
        {
            _filteredData = _source.ToList();
            OnFilterUpdated?.Invoke(_filteredData);
            Redraw();
        }

        #region Page Navigation
        public void First()
        {
            PageIndex = 0;
        }

        public void Last()
        {
            PageIndex = NumPages - 1;
        }

        public void Next()
        {
            PageIndex++;
        }

        public void Previous()
        {
            PageIndex--;
        }

        public void GoTo(T data)
        {
            // Find index of item in the list
            var index = _filteredData.ToList().IndexOf(data);
            if (index < 0) return;
            
            // Find the page
            var page = index / _itemsPerPage;
            PageIndex = page;
        }
        #endregion

        #region Selection
        public void Select(IEnumerable<T> items, bool shouldAppend = false)
        {
            if (shouldAppend)
            {
                foreach (var item in items.ToArray())
                {
                    if (_selectedItems.Any(i => IsDataEqual(item, i))) continue;
                    _selectedItems.Add(item);
                    OnDataSelected(item, true);
                    OnSelected?.Invoke(item, true);
                }
            }
            else
            {
                // Deselect previous items that aren't included in the new selection
                var toDeselect = _selectedItems.Where(s => !items.Any(i => IsDataEqual(s, i))).ToArray();
                foreach (var item in toDeselect)
                {
                    OnDataSelected(item, false);
                    OnSelected?.Invoke(item, false);
                }
                
                // Overwrite selection
                _selectedItems = items.ToList();
                foreach (var item in _selectedItems)
                {
                    OnDataSelected(item, true);
                    OnSelected?.Invoke(item, true);
                }
            }
            
            RefreshCurrentItems();
        }

        public void Select(T item)
        {
            if (_selectedItems.Any(i => IsDataEqual(item, i))) return;

            _selectedItems.Add(item);
            OnDataSelected(item, true);
            OnSelected?.Invoke(item, true);
            RefreshCurrentItems();
        }

        public void Deselect(T item)
        {
            if (!_selectedItems.Any(i => IsDataEqual(item, i))) return;
            
            _selectedItems.RemoveAll(i => IsDataEqual(item, i));
            OnDataSelected(item, false);
            OnSelected?.Invoke(item, false);
            RefreshCurrentItems();
        }
        
        public void SelectAll()
        {
            _selectedItems = _filteredData.ToList();
            foreach (var item in _selectedItems)
            {
                OnDataSelected(item, true);
                OnSelected?.Invoke(item, true);
            }
            RefreshCurrentItems();
        }

        public void ClearSelection()
        {
            var previouslySelected = _selectedItems.ToList();
            _selectedItems = new List<T>();
            foreach (var item in previouslySelected)
            {
                OnDataSelected(item, false);
                OnSelected?.Invoke(item, false);
            }
            RefreshCurrentItems();
        }
        #endregion

        private void Awake()
        {
            // Hide item template (if applicable)
            if (_itemPrefab.gameObject.scene.IsValid()) _itemPrefab.gameObject.SetActive(false);
            if (_pageButtonTemplate && _pageButtonTemplate.gameObject.scene.IsValid()) _pageButtonTemplate.gameObject.SetActive(false);
            
            // Add listener to static page buttons
            _customPreviousButton?.Clicked.AddListener(OnPageButtonClicked);
            _customNextButton?.Clicked.AddListener(OnPageButtonClicked);
            _customPreviousButton?.gameObject.SetActive(false);
            _customNextButton?.gameObject.SetActive(false);
            
            // Redraw page if number of items per page is changed
            if (_itemsPerPageInput)
            {
                _itemsPerPageInput.text = _itemsPerPage.ToString();

                _itemsPerPageInput.onEndEdit.AddListener(text =>
                {
                    if (EventSystem.current.currentSelectedGameObject != _itemsPerPageInput.gameObject) return;
                    if (!int.TryParse(text, out var numItems))
                    {
                        _itemsPerPageInput.text = _itemsPerPage.ToString();
                        return;
                    }
                    if (numItems <= 0)
                    {
                        _itemsPerPageInput.text = _itemsPerPage.ToString();
                        return;
                    }

                    _itemsPerPage = numItems;
                    Redraw();
                });
            }
        }

        private void Start()
        {
            DrawPagePoll().Forget();
            RefreshCurrentItemsPoll().Forget();
            OnInitialized();
        }
        
        private async UniTaskVoid DrawPagePoll()
        {
            while (!ReferenceEquals(this, null))
            {
                if (!_shouldDrawPage)
                {
                    await UniTask.Yield();
                    continue;
                }
                _shouldDrawPage = false;
                
                DrawPage();
            }
        }

        private void DrawPage()
        {
            // Recreate items if needed
            CreateItems();
            
            // Update filter. Ensure that filtered data only contains data from the source
            _filteredData.RemoveAll(data => !_source.Any(d => IsDataEqual(d, data)));
            
            // Show filtered data for the selected page index
            var dataToDisplay = _paginationEnabled
                ? _filteredData.Skip(PageIndex * _itemsPerPage).Take(_itemsPerPage).ToList()
                : _filteredData.ToList();
            for (var i = 0; i < dataToDisplay.Count; i++)
            {
                var data = dataToDisplay[i];
                _items[i].Show(data, _selectedItems.Any(item => IsDataEqual(item, data)));
            }
            OnDataShown(dataToDisplay);
            
            // Hide excess items
            var itemsToHide = _items.Skip(dataToDisplay.Count).ToList();
            itemsToHide.ForEach(i => i.gameObject.SetActive(false));

            DrawPageButtons();
        }

        private void CreateItems()
        {
            // Don't recreate items when number of items per page is the same
            if (_paginationEnabled && _itemsPerPage == _items.Count) return;
            if (!_paginationEnabled && _filteredData.Count == _items.Count) return;
            
            // Delete previous items
            foreach (var item in _items)
            {
                item.Pool();
                _pooledItems.Push(item);
            }
            _items.Clear();
            
            // Recreate items
            var numItemsToCreate = _paginationEnabled 
                ? _itemsPerPage 
                : _filteredData.Count;
            for (var i = 0; i < numItemsToCreate; i++)
            {
                if (!_pooledItems.TryPop(out var item))
                {
                    item = Instantiate(_itemPrefab, _itemContainer, false);
                    
                    item.OnClicked += data =>
                    {
                        OnDataClicked(data);
                        OnClicked?.Invoke(data);
                    };
                    item.OnSelected += (data, selected) =>
                    {
                        if (selected) _selectedItems.Add(data);
                        else _selectedItems.Remove(data);
                    
                        OnDataSelected(data, selected);
                        OnSelected?.Invoke(data, selected);
                    };
                }
                
                item.transform.SetSiblingIndex(i + _itemContainer.childCount - 1);
                item.gameObject.SetActive(true);
                _items.Add(item);
                
                OnListItemCreated(item);
            }
            
            OnListItemsCreated(_items);
        }

        private void DrawPageButtons()
        {
            if (!_paginationEnabled) return;
            
            // Determine which pages to show
            var startIndex = (PageIndex / _maxVisiblePageButtons) * _maxVisiblePageButtons;
            var endIndex = Math.Min(startIndex + _maxVisiblePageButtons - 1, NumPages - 1);

            // Clear previous
            foreach (var pageButton in _pageButtons)
            {
                pageButton.gameObject.SetActive(false);
                _pooledPageButtons.Push(pageButton);
            }
            _pageButtons.Clear();
            
            // Don't create buttons if there's only one page
            if (_hidePageButtonsForSinglePage && NumPages == 1)
            {
                _customPreviousButton?.gameObject.SetActive(false);
                _customNextButton?.gameObject.SetActive(false);
                return;
            }
            
            // Show previous button
            if (PageIndex > 0 && NumPages > 1)
            {
                if (_customPreviousButton)
                {
                    _customPreviousButton?.Init(PageIndex - 1, PaginationButtonType.Previous, false);
                    _customPreviousButton?.gameObject.SetActive(true);
                }
                else
                {
                    AddPageButton(PageIndex - 1, PaginationButtonType.Previous);
                }
            }
            else _customPreviousButton?.gameObject.SetActive(false);

            // Show first button
            if (startIndex > 0)
            {
                // First page
                AddPageButton(0, PaginationButtonType.First);

                // Truncated page
                if (_showPageTruncation)
                    AddPageButton(Math.Max(PageIndex - _maxVisiblePageButtons, 1), PaginationButtonType.Truncated);
            }

            // Show index buttons
            for (var i = startIndex; i < endIndex + 1; i++)
            {
                AddPageButton(i, PaginationButtonType.Index);
            }

            // Show last button
            if (endIndex < NumPages - 1)
            {
                // Truncated page
                if (_showPageTruncation)
                    AddPageButton(Math.Min(PageIndex + _maxVisiblePageButtons, NumPages - 1), PaginationButtonType.Truncated);
                
                // Last page
                AddPageButton(NumPages - 1, PaginationButtonType.Last);
            }
            
            // Show next button
            if (PageIndex < NumPages - 1 && NumPages > 1)
            {
                if (_customNextButton)
                {
                    _customNextButton?.Init(PageIndex + 1, PaginationButtonType.Next, false);
                    _customNextButton?.gameObject.SetActive(true);
                }
                else
                {
                    AddPageButton(PageIndex + 1, PaginationButtonType.Next);
                }
            }
            else _customNextButton?.gameObject.SetActive(false);
        }

        private PaginationButton AddPageButton(int index, PaginationButtonType type)
        {
            // Get from pool or instantiate
            if (!_pooledPageButtons.TryPop(out var pageButton))
            {
                pageButton = Instantiate(_pageButtonTemplate, _pageButtonContainer, false);
                pageButton.Clicked.AddListener(OnPageButtonClicked);
            }

            pageButton.Init(index, type, type == PaginationButtonType.Index && index == PageIndex);
            pageButton.gameObject.SetActive(true);
            pageButton.transform.SetSiblingIndex(index + _pageButtonContainer.childCount - 1);
            _pageButtons.Add(pageButton);
            
            return pageButton;
        }

        private void OnValidate()
        {
            // Automatically set the item container to itemPrefab's parent if they're in the same scene.
            if (_itemPrefab && (_itemPrefab.transform.parent.gameObject.scene == gameObject.scene))
            {
                _itemContainer = _itemPrefab.transform.parent as RectTransform;
            }
            
            if (_pageButtonTemplate && (_pageButtonTemplate.transform.parent.gameObject.scene == gameObject.scene))
            {
                _pageButtonContainer = _pageButtonTemplate.transform.parent as RectTransform;
            }
        }
        
        private void OnPageButtonClicked(PaginationButton button)
        {
            PageIndex = button.Index;
        }
    }
}

