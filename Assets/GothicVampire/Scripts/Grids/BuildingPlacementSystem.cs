using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Player.Inputs.Entity;
using Sylpheed.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Handles grid-based building placement and communication between GridHighlighter and BuildingManager.
    /// </summary>
    public class BuildingPlacementSystem : MonoBehaviour, IBuildingService
    {
        [SerializeField] private BuildingHighlighter _buildingHighlighter;
        [SerializeField] private GridSystem _gridSystem;
        private IEntitySelectorService _entitySelector;
        private IEntityRelocatorService _entityRelocator;
        private IWorldVisualEffectsService _worldVisualEffectsService;

        private BuildingData _buildingToPlaceGrid;
        private bool _isRelocatingBuilding = false;
        private Quaternion _relocatedOriginalRotation;
        private GridCoord _relocatedOriginalPosition;
        private bool _isPreplacingBuilding = false;
        private bool _endlessPreplacing = false;

        private Vector3 _originalOffset;
        private Vector2 _lastBuildingGridSize;
        private GameObject _lastGhostPrefab;

        private BuildingManager _buildingManager;
        private Wallet _wallet;
        public bool IsPreplacingBuilding => _isPreplacingBuilding;

        public void Awake()
        {
            ServiceLocator.Register<IBuildingService>(this);
            ServiceLocator.Register(this);
        }

        public void Start()
        {
            _entitySelector = ServiceLocator.Get<IEntitySelectorService>();
            _entityRelocator = ServiceLocator.Get<IEntityRelocatorService>();
            _wallet = ServiceLocator.Get<World>().Player.GetService<Wallet>();
            _worldVisualEffectsService = ServiceLocator.Get<IWorldVisualEffectsService>();

            if (_buildingHighlighter != null)
            {
                _buildingHighlighter.EvtOnFinishedBuildingPlacing.AddListener(FinalizeBuildingPlacement);
                _buildingHighlighter.EvtOnCancelBuildingPlacing.AddListener(CancelBuildingPlacement);
            }

            if (_buildingManager == null)
            {
                _buildingManager = ServiceLocator.Get<World>().Player.GetService<BuildingManager>();
            }

            if(_wallet != null)
            {
                _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            }
        }

        public void OnDestroy()
        {
            ServiceLocator.Remove<IBuildingService>();
            ServiceLocator.Remove(this);
            _buildingHighlighter.EvtOnFinishedBuildingPlacing.RemoveListener(FinalizeBuildingPlacement);
            _buildingHighlighter.EvtOnCancelBuildingPlacing.RemoveListener(CancelBuildingPlacement);
        }

        #region Public Methods

        public void PlaceBuilding(BuildingData buildingData)
        {
            if (!_buildingManager.CanPurchase(buildingData)) return;

            if (_entitySelector != null)
            {
                _entitySelector.UnselectCurrentEntity();
            }

            if (_isPreplacingBuilding)
            {
                CancelBuildingPlacement();
            }

            _buildingToPlaceGrid = buildingData;
            _isRelocatingBuilding = false;
            PrePlaceBuilding(buildingData.GridSize, buildingData.Ghost);
        }

        public void RelocateBuilding()
        {
            SelectableEntity entity = _entitySelector.GetSelectedEntity();
            entity.EvtEntityRelocating?.Invoke();

            Building building = entity.GetComponentInParent<Building>();

            _buildingToPlaceGrid = building.Data;
            _relocatedOriginalRotation = building.gameObject.transform.rotation;
            _isRelocatingBuilding = true;
            _relocatedOriginalPosition = building.GridPosition;
            RelocateBuilding(_buildingToPlaceGrid.GridSize, building.gameObject, building.GridPosition, building.Rotation);
        }

        // Temporary, to improve later.
        // Idea is to Receive these datas from a saved Data file, and simply instantiate the models elsewhere.
        public Vector3 ProjectGridToWorldPosition(GridCoord gridPos, GridCoord size, Quaternion rotation)
        {
            return Vector3.zero;
        }

        public void Rotate(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed && (_isPreplacingBuilding || _isRelocatingBuilding))
            {
                _buildingHighlighter.RotateFootprintCW();
            }
        }

        public void UpdateEndlessPlacement(InputAction.CallbackContext context)
        {
            _endlessPreplacing = context.phase == InputActionPhase.Performed && (_isPreplacingBuilding);
        }

        public Vector3 GridCoordToWorldPosition(GridCoord gridCoord)
        {
            return _gridSystem.Grid.GetWorldCenter(gridCoord);
        }

        public GridCoord WorldToGridCoord(Vector3 worldPosition)
        {
            return _gridSystem.Grid.GetCoordFromWorld(worldPosition);
        }

        public Vector3 GetWorldGridRotatedPosition(GridCoord gridCoord, Quaternion rotation)
        {
            return _gridSystem.Grid.GetWorldCenterRotated(gridCoord, rotation);
        }

        public bool IsRelocatingOrPlacingBuilding()
        {
            return _isPreplacingBuilding || _isRelocatingBuilding;
        }

        #endregion

        #region Private Methods

        private void OnWalletUpdated(Wallet wallet)
        {
            if (_buildingToPlaceGrid == null) return;

            var canPurchase = _buildingManager.CanPurchase(_buildingToPlaceGrid);
            _buildingHighlighter.NotEnoughResources(!canPurchase);
        }

        private void FinalizeBuildingPlacement(GridCoord gridCoord, Quaternion rotation)
        {
            if (_isRelocatingBuilding)
            {
                var selectedEntity = _entitySelector.GetSelectedEntity();

                var existingBuildingService = selectedEntity.GetComponent<Building>();
                // Update building Data
                existingBuildingService.Relocate(gridCoord, rotation);

                _buildingHighlighter.SetOccupant(selectedEntity.gameObject);
                _isRelocatingBuilding = false;
            }
            else
            {
                var newBuilding = _buildingManager.Build(_buildingToPlaceGrid, gridCoord, rotation);

                if (newBuilding == null) return;

                var testValue = GridCoordToWorldPosition(gridCoord);
                //Debug.Log($"[GridPlacementSystem.cs/FinalizeBuildingPlacement] Grid Position: {gridCoord} | world Position:{testValue}");

                _worldVisualEffectsService.SetVisualModelsToNormal();
                _buildingHighlighter.SetOccupant(newBuilding.gameObject);
            }

            CheckEndlessPlacement();
        }

        private void CancelBuildingPlacement()
        {
            if (!_isRelocatingBuilding)
            {
                _buildingToPlaceGrid = null;
                _isPreplacingBuilding = false;
                _buildingHighlighter.DeleteCurGhostPrefab();
            }
            else
            {
                _buildingHighlighter.ReturnToOriginalPosition();
                _isRelocatingBuilding = false;
            }

            _worldVisualEffectsService.SetVisualModelsToNormal();
        }


        private void CheckEndlessPlacement()
        {
            if (_endlessPreplacing)
            {
                PrePlaceBuilding(_lastBuildingGridSize, _lastGhostPrefab);
            }
            else
            {
                if (_isPreplacingBuilding)
                {
                    _buildingHighlighter.DeleteCurGhostPrefab();
                }

                _buildingHighlighter.DisableHighlight();
                _buildingToPlaceGrid = null;
                _isPreplacingBuilding = false;
                _worldVisualEffectsService.SetVisualModelsToNormal();
                _entitySelector.UnselectCurrentEntity();

            }
        }

        private void PrePlaceBuilding(Vector2 buildingGridSize, GameObject ghostPrefab)
        {
            if (_isPreplacingBuilding) return;

            _isPreplacingBuilding = true;
            
            _lastBuildingGridSize = buildingGridSize;
            _lastGhostPrefab = ghostPrefab;

            _buildingHighlighter.SetHighlightGridSize(buildingGridSize);
            _buildingHighlighter.SetPrePlaceGhostBuilding(ghostPrefab);
            _buildingHighlighter.EnableHighlight();
            _worldVisualEffectsService.SetVisualModelsToGhost();
        }

        private void RelocateBuilding(Vector2 buildingGridSize, GameObject buildingGameObject, GridCoord gridPosition, Quaternion rotation)
        {
            _buildingHighlighter.SetHighlightGridSize(buildingGridSize);
            _buildingHighlighter.EnableHighlight();
            _buildingHighlighter.StartRelocatingBuilding(buildingGameObject, buildingGridSize, rotation, gridPosition);
            _worldVisualEffectsService.SetVisualModelsToGhost();
        }

        #endregion

        #region IGridService
        Vector3 IBuildingService.GridToWorldPosition(GridCoord gridPos) => GridCoordToWorldPosition(gridPos);
        GridCoord IBuildingService.WorldToGridPosition(Vector3 worldPos) => WorldToGridCoord(worldPos);
        Vector3 IBuildingService.RotatedPosition(GridCoord gridPos, Quaternion rotation) => GetWorldGridRotatedPosition(gridPos, rotation);
        void IBuildingService.RelocateBuilding() => RelocateBuilding();
        Vector3 IBuildingService.ProjectGridToWorldPosition(GridCoord gridPos, GridCoord size, Quaternion rotation) => ProjectGridToWorldPosition(gridPos, size, rotation);
        bool IBuildingService.IsRelocatingOrPlacingBuilding() => IsRelocatingOrPlacingBuilding();
        #endregion
    }
}
