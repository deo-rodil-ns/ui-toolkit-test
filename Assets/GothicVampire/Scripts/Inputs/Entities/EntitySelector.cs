using GothicVampire.Grids;
using Sylpheed.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GothicVampire.Player.Inputs.Entity
{
    public class EntitySelector : MonoBehaviour, IEntitySelectorService
    {
        private UnityEvent<SelectableEntity> EvtEntitySelected { get; } = new();
        private UnityEvent EvtDoubleClickedUpdate { get; } = new();
        private UnityEvent EvtEntityUnSelected { get; } = new();

        [SerializeField] private CameraMovement _cameraMovement;
        private List<SelectableEntity> _curSelectedEntities;
        private IBuildingService _gridPlacementSystem;
        private IRoadService _roadPlacementSystem;
        private List<SelectableEntity> _selectableEntities = new List<SelectableEntity>();


        void Awake()
        {
            ServiceLocator.Register<IEntitySelectorService>(this);
            _curSelectedEntities = new List<SelectableEntity>();
        }

        private void Start()
        {
            _gridPlacementSystem = ServiceLocator.Get<IBuildingService>();
            _roadPlacementSystem = ServiceLocator.Get<IRoadService>();
        }

        void Update()
        {
            if (_curSelectedEntities.Count > 0 && 
                Mouse.current?.rightButton.wasPressedThisFrame == true && 
                !_gridPlacementSystem.IsRelocatingOrPlacingBuilding() &&
                !_roadPlacementSystem.IsPreplacingRoad())
            {
                UnselectEntities();
            }
        }

        public void RegisterEntity(SelectableEntity selectableEntity)
        {
            _selectableEntities.Add(selectableEntity);
        }

        public void ResignEntity(SelectableEntity selectableEntity)
        {
            _selectableEntities.Remove(selectableEntity);
        }

        public void SelectEntity(SelectableEntity entityView)
        {
            if (_curSelectedEntities.Count > 0 && _selectableEntities.Contains(entityView))
            {
                UnselectEntities();
            }

            _curSelectedEntities.Add(entityView);
            
            if (_cameraMovement.FocusOnEntityEnabled)
            {
                _cameraMovement.MoveToPosition(_curSelectedEntities[0].transform.position);
            }

            EvtEntitySelected?.Invoke(_curSelectedEntities[0]);
        }

        public void UnselectEntity(SelectableEntity entity)
        {
            if (!_curSelectedEntities.Contains(entity))
            {
                Debug.LogWarning($"Trying to UNSELECT an entity that is not selected: {entity.gameObject.name}");
                return;
            }

            if(_curSelectedEntities.Count > 0)
            {
                _curSelectedEntities.ForEach(entity => entity.RemoveHighlight());
                _curSelectedEntities.Clear();
            }

            // EntityHud - removes UI
            EvtEntityUnSelected?.Invoke();
        }

        public void DoubleClicked(SelectableEntity entity)
        {
            var allSelectedEntity = new List<SelectableEntity>();

            switch (entity.Type)
            {
                case EntityType.None:
                    break;
                case EntityType.Building:
                    break;
                case EntityType.Terrain:
                    break;
                case EntityType.Road:
                    allSelectedEntity = _roadPlacementSystem.GetStreet(entity);
                    
                    allSelectedEntity.ForEach(road =>
                    {
                        if (_curSelectedEntities.Contains(road)) return;

                        road.AddHighlight();
                        _curSelectedEntities.Add(road);                        
                    });
                    

                    Debug.Log($"Street Count:{allSelectedEntity.Count}");

                    break;
                default:
                    break;
            }

            EvtDoubleClickedUpdate.Invoke();
        }

        //ToDo: Temporary
        public SelectableEntity GetSelectedEntity()
        {
            if(_curSelectedEntities.Count <= 0) return null;

            return _curSelectedEntities[0];
        }

        private void UnselectEntities()
        {
            if (_curSelectedEntities.Count <= 0) return;

            foreach (var entity in _curSelectedEntities.ToList())
            {
                entity.Unselect();
            }

            EvtEntityUnSelected?.Invoke();

            _curSelectedEntities.Clear();
        }

        #region IEntitySelectorService

        UnityEvent<SelectableEntity> IEntitySelectorService.EvtEntitySelected => EvtEntitySelected;
        UnityEvent IEntitySelectorService.EvtEntityUnSelected => EvtEntityUnSelected;
        UnityEvent IEntitySelectorService.EvtDoubleClickedUpdate => EvtDoubleClickedUpdate;
        IReadOnlyList<SelectableEntity> IEntitySelectorService.GetSelectedEntities => _curSelectedEntities;
        SelectableEntity IEntitySelectorService.GetSelectedEntity() => GetSelectedEntity();
        void IEntitySelectorService.RegisterEntity(SelectableEntity entity) => RegisterEntity(entity);
        void IEntitySelectorService.ResignEntity(SelectableEntity entity) => ResignEntity(entity);
        void IEntitySelectorService.SelectEntity(SelectableEntity entity) => SelectEntity(entity);
        void IEntitySelectorService.DoubleClicked(SelectableEntity entity) => DoubleClicked(entity);
        void IEntitySelectorService.UnselectEntity(SelectableEntity entity) => UnselectEntity(entity);
        void IEntitySelectorService.UnselectCurrentEntity() => UnselectEntities();
        #endregion
    }
}
