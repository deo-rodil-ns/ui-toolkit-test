using GothicVampire.Buildings;
using GothicVampire.Grids;
using GothicVampire.Roads;
using Sylpheed.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace GothicVampire.Player.Inputs.Entity
{
    public enum EntityType
    {
        None,
        Building,
        Terrain,
        Road,
        Villager,
    }

    [RequireComponent(typeof(GridEntity))]
    public class SelectableEntity : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private EntityType _type;
        [SerializeField] private float _doubleClickTimeCheck = 0.25f;

        #endregion

        #region UnityEvents

        public UnityEvent EvtEntitySelected { get; } = new UnityEvent();
        public UnityEvent EvtEntityDoubleClicked { get; } = new UnityEvent();
        public UnityEvent EvtEntityUnselected { get; } = new UnityEvent();
        public UnityEvent EvtEntityRelocated { get; } = new UnityEvent();
        public UnityEvent EvtEntityRelocating { get; } = new UnityEvent();

        #endregion


        public bool Selected { get; private set; }
        public EntityType Type => _type;

        private ModelVisualInputEffects _visualEffects;
        private InputSystemUIInputModule _uiModule;

        private Camera _mainCamera;

        private GridEntity _gridEntity;
        private IEntitySelectorService _entitySelector;
        private IBuildingService _gridPlacementSystem;
        private IRoadService _roadPlacementSystem;

        private float reselectDelayCount = 0.05f;
        private bool _delayActivated = false;

        private bool _checkDoubleClick = false;
        private float _doubleClickCount = 0.0f;

        private void Awake()
        {
            _uiModule = FindFirstObjectByType<InputSystemUIInputModule>();
        }

        void Start()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            _gridEntity = GetComponent<GridEntity>();
            _entitySelector = ServiceLocator.Get<IEntitySelectorService>();
            _gridPlacementSystem = ServiceLocator.Get<IBuildingService>();
            _roadPlacementSystem = ServiceLocator.Get<IRoadService>();

            if (_entitySelector != null)
            {
                _entitySelector.RegisterEntity(this);
            }
        }

        private void OnDestroy()
        {
            if(_entitySelector != null)
            {
                _entitySelector.ResignEntity(this);
            }
        }

        void Update()
        {
            if (_checkDoubleClick && Selected)
            {
                _doubleClickCount += Time.deltaTime;
                if (_doubleClickCount > _doubleClickTimeCheck)
                {
                    ResetDoubleClick();
                }
            }

            // Detect left mouse click
            if (Mouse.current?.leftButton.wasPressedThisFrame == true && !_delayActivated)
            {
                var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out var hit) && 
                    hit.collider.transform.IsChildOf(transform) &&
                    !IsPointerOverUI() &&
                    !_gridPlacementSystem.IsRelocatingOrPlacingBuilding() &&
                    !_roadPlacementSystem.IsPreplacingRoad())
                {
                    if (!_checkDoubleClick)
                    {
                        Select();
                        StartDoubleClickCheck();
                    }
                    else
                    {
                        DoubleClicked();
                    }
                }
            }
        }

        public void AddHighlight()
        {
            EvtEntitySelected?.Invoke();
        }

        public void RemoveHighlight()
        {
            EvtEntityUnselected?.Invoke();
        }

        public void Select()
        {
            Selected = true;
            _entitySelector.SelectEntity(this);
            AddHighlight();
        }

        public void DoubleClicked()
        {
            EvtEntityDoubleClicked.Invoke();
            _entitySelector.DoubleClicked(this);
        }

        public void Unselect()
        {
            ResetDoubleClick();
            Selected = false;
            _delayActivated = true;

            _entitySelector.UnselectEntity(this);
            // Events - changes visuals
            RemoveHighlight();

            StartCoroutine(DelayReselecting());
        }

        #region Private Methods

        private EntityType GetEntityType
        {
            get
            {
                if (_gridEntity == null)
                    return EntityType.None;
                return _gridEntity.Type;
            }
        }

        private bool IsPointerOverUI()
        {
            if (_uiModule == null) return false;

            // Mouse input
            if (Mouse.current != null)
            {
                int mousePointerId = Pointer.current.deviceId; // Mouse pointer id
                return _uiModule.IsPointerOverGameObject(mousePointerId);
            }

            // Touch input
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                int touchPointerId = Touchscreen.current.deviceId;
                return _uiModule.IsPointerOverGameObject(touchPointerId);
            }

            return false;
        }

        private void StartDoubleClickCheck()
        {
            _checkDoubleClick = true;
            _doubleClickCount = 0;
        }

        private void ResetDoubleClick()
        {
            _checkDoubleClick = false;
            _doubleClickCount = 0;
        }

        IEnumerator DelayReselecting()
        {
            yield return new WaitForSeconds(reselectDelayCount);
            _delayActivated = false;
        }

        #endregion
    }
}
