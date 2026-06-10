// GridHighlighter.cs
using GothicVampire.Player.Inputs.Entity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Handles visual highlighting of grid cells and placement interactions.
    /// Depends on a GridSystem in the same GameObject.
    /// </summary>
    [RequireComponent(typeof(GridSystem))]
    public class BuildingHighlighter : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Invoked when a placement action is completed successfully.
        /// </summary>
        [HideInInspector]
        public UnityEvent<GridCoord, Quaternion> EvtOnFinishedBuildingPlacing { get; } = new();
        public UnityEvent EvtOnCancelBuildingPlacing { get; } = new();

        #endregion

        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Transform highlightContainer;
        [Header("Highlight")]
        [SerializeField] private GridVisualizer _gridVisualizer;
        [Tooltip("Prefab for a single highlighted tile (e.g., a thin quad with transparent material).")]
        [SerializeField] private Transform _highlightTilePrefab;
        [Header("State")]
        [SerializeField] private GameObject _curRelocatedEntity;
        [SerializeField] private GameObject _curRelocatedModel;
        [SerializeField] private bool _highlightEnabled = false;
        [SerializeField] private bool _isGhostPrefab = false;
        #endregion

        #region Private Fields

        private GridSystem _grid;

        private readonly List<GridHighlight> _tilePool = new();
        private readonly List<GridCoord> _currentCells = new();
        private readonly List<GridCoord> _originalCells = new List<GridCoord>();
        private Quaternion _originalRotation;
        private Vector3 _originalPosition;
        private GridCoord _originalAnchor;
        private Vector2Int _footprint = new(1, 1); // width (x) by height (y) in grid cells
        private Quaternion _footprintRotation = Quaternion.identity;
        private float _cacheYRotation;

        private GridCoord _lastAnchorCell;
        private GameObject _ghostAnchor;
        private bool _notEnoughResources = false;
        private Camera _mainCam;
        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _mainCam = Camera.main;
            _grid = GetComponent<GridSystem>();
            DeactivateAllTiles();
        }

        private void Update()
        {
            if (!_highlightEnabled || _mainCam == null || _grid.Grid == null)
                return;

            if (!TryGetPointer(out Vector2 screenPos, out bool clickPressedThisFrame))
                return;

            // Cancel Building
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            var ray = _mainCam.ScreenPointToRay(screenPos);

            if (!_grid.TryGetCoordFromRay(ray, out var hoverCoord, out _))
            {
                DeactivateAllTiles();
                return;
            }

            UpdateBuildingHighlights(hoverCoord, clickPressedThisFrame);
        }

        #endregion

        #region Public Methods

        public void NotEnoughResources(bool newState)
        {
            _notEnoughResources = newState;
        }

        /// <summary>
        /// Enables the highlight system.
        /// </summary>
        public void EnableHighlight()
        {
            _gridVisualizer.ShowVisuals(true);
            _highlightEnabled = true;
        }

        /// <summary>
        /// Disables the highlight system and hides all tiles.
        /// </summary>
        public void DisableHighlight()
        {
            _highlightEnabled = false;
            _gridVisualizer.ShowVisuals(false);
            DeactivateAllTiles();
        }

        /// <summary>
        /// Creates the ghost building
        /// </summary>
        /// <param name="newPrefab"></param>
        public void SetPrePlaceGhostBuilding(GameObject newPrefab)
        {
            _curRelocatedEntity = Instantiate(newPrefab, transform);
            _isGhostPrefab = true;
            _ghostAnchor = _curRelocatedEntity.transform.GetChild(0).gameObject;

            if (Mathf.Abs(_cacheYRotation) > 0)
            {
                _footprint = new Vector2Int(_footprint.y, _footprint.x);
            }

            UpdateFootPrintRotation("SetPrePlaceGhostBuilding");
            UpdateGhostAnchorOffsetBasedOnRotation();
        }

        public void StartRelocatingBuilding(GameObject gameObject, Vector2 gridSize, Quaternion rotation, GridCoord anchor)
        {
            if (_originalCells.Count > 0) _originalCells.Clear();

            _isGhostPrefab = false;
            _curRelocatedModel = gameObject;

            // Normalize/snap to 0/90 if your game only supports that
            bool is90 = rotation.eulerAngles.y > 0;

            var baseFootprint = new Vector2Int(
                Mathf.RoundToInt(gridSize.x),
                Mathf.RoundToInt(gridSize.y)
            );


            _originalPosition = gameObject.transform.localPosition;
            
            // Footprint Grids
            _footprint = is90
                ? new Vector2Int(baseFootprint.y, baseFootprint.x)
                : baseFootprint; ;

            _originalAnchor = anchor;
            _originalRotation = rotation;

            _footprintRotation = rotation;

            ClearPreviousOccupancy(gameObject, gridSize, rotation, anchor);

            _curRelocatedEntity = gameObject;
        }

        public void ReturnToOriginalPosition()
        {
            if (_curRelocatedEntity == null || _originalCells.Count == 0)
                return;

            var anchor = _originalCells[0]; // or _originalAnchor
            var anchorCenter = _grid.Grid.GetWorldCenter(anchor);
            var cellSize = _grid.Grid.CellSize;

            // Use the ORIGINAL footprint to compute the center offset
            _curRelocatedEntity.transform.localPosition = _originalPosition;

            _curRelocatedModel.transform.localRotation = _originalRotation;

            foreach (var coord in _originalCells)
            {
                if (_grid.Grid.InBounds(coord))
                {
                    var cell = _grid.Grid.Get(coord);
                    cell.Occupant = _curRelocatedEntity;
                }
            }

            _originalCells.Clear();
        }




        public void DeleteCurGhostPrefab()
        {
            if (_isGhostPrefab)
            {
                Destroy(_curRelocatedEntity);
            }
        }

        /// <summary>
        /// Sets the footprint size in grid cells (e.g., (2,2) for a 2x2 area).
        /// </summary>
        public void SetHighlightGridSize(Vector2 gridSize)
        {
            _footprint = new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(gridSize.x)),
                Mathf.Max(1, Mathf.RoundToInt(gridSize.y))
            );
            EnsurePool(_footprint.x * _footprint.y);
        }

        /// <summary>
        /// Rotates the footprint 90° clockwise (grid-aligned, swaps X/Y).
        /// </summary>
        public void RotateFootprintCW()
        {
            // Rotate Determine the next rotation: 0 -> 90 -> 0
            float currentY = _footprintRotation.eulerAngles.y;
            _cacheYRotation = Mathf.Approximately(currentY, 0f) ? -90f : 0f;

            bool isSwappingPosition = _cacheYRotation != currentY;

            if (isSwappingPosition)
            {
                _footprint = new Vector2Int(_footprint.y, _footprint.x);
            }

            UpdateFootPrintRotation("RotateFootprintCW");
        }

        public void SetOccupant(GameObject gameobject)
        {
            foreach(var coord in _currentCells)
            {
                var cell = _grid.Grid.Get(coord);
                cell.Occupant = gameobject;
                cell.OccupantEntity = EntityType.Building;
                cell.Walkable = false;
            }

            _grid.ComputePenaltyFields();
        }

        #endregion

        #region Private Helpers

        private void UpdateFootPrintRotation(string from)
        {
            EnsurePool(_footprint.x * _footprint.y);

            // Apply rotation
            _footprintRotation = Quaternion.Euler(0f, _cacheYRotation, 0f);

            // Update ghost prefab rotation
            if (_curRelocatedEntity != null)
            {
                if (_isGhostPrefab)
                {
                    UpdateGhostAnchorOffsetBasedOnRotation();
                }
                else
                {
                    _curRelocatedEntity.transform.localRotation = _footprintRotation;
                }
            }

        }

        private void ClearPreviousOccupancy(GameObject building, Vector2 gridSize, Quaternion rotation, GridCoord anchor)
        {
            // Ensure grid exists
            if (_grid == null || _grid.Grid == null)
                return;

            // Determine footprint size based on rotation
            Vector2Int footprint = new Vector2Int(
                Mathf.RoundToInt(gridSize.x),
                Mathf.RoundToInt(gridSize.y)
            );

            float yRot = rotation.eulerAngles.y;
            if (Mathf.Approximately(yRot, 90f) || Mathf.Approximately(yRot, 270f))
            {
                // Swap width/height if rotated 90° or 270°
                footprint = new Vector2Int(footprint.y, footprint.x);
            }

            // Now clear all cells under that footprint
            for (int dx = 0; dx < footprint.x; dx++)
            {
                for (int dy = 0; dy < footprint.y; dy++)
                {
                    GridCoord coord = new GridCoord(anchor.x + dx, anchor.y + dy);

                    if (_grid.Grid.InBounds(coord))
                    {
                        if (_grid.Grid.TryGet(coord, out var cell) && cell.Occupant == building)
                            cell.Occupant = null;

                        _originalCells.Add(coord);
                    }
                }
            }
        }

        private void UpdateBuildingHighlights(GridCoord hoverCoord, bool clickPressedThisFrame)
        {
            // Compute top-left (anchored) rect that fits within the grid
            var anchor = FitAnchorToBounds(hoverCoord, _footprint);

            if (_lastAnchorCell != Vector2.zero && _lastAnchorCell.x != anchor.x && _lastAnchorCell.y != anchor.y)
            {
                _lastAnchorCell = anchor;
            }
            else if (_lastAnchorCell == Vector2.zero)
            {
                _lastAnchorCell = anchor;
            }

            BuildFootprintCells(anchor, _footprint, _currentCells);

            // Make sure pool has enough tiles
            EnsurePool(_currentCells.Count);

            // Position active highlight tiles
            PositionHighlightTiles();

            CurEntityPrefabFollow(anchor);

            // Attempt placement if user clicked
            if (clickPressedThisFrame && !IsMouseHoveringUi())
            {
                if (AllCellsFree(_currentCells))
                {
                    EvtOnFinishedBuildingPlacing.Invoke(anchor, _footprintRotation);
                }
                else
                {
                    // Optional: visual feedback for blocked placement
                    // StartCoroutine(FlashTiles(Color.red));
                }
            }
        }

        private void PositionHighlightTiles()
        {
            // Position active highlight tiles
            for (int i = 0; i < _tilePool.Count; i++)
            {
                if (i < _currentCells.Count)
                {
                    var coord = _currentCells[i];
                    var cell = _grid.Grid.Get(coord);
                    var center = _grid.Grid.GetWorldCenter(coord);
                    var tile = _tilePool[i];
                    tile.position = new Vector3(center.x, tile.position.y, center.z);

                    var isOccupied = cell.Occupant != null || cell.Unbuildable || _notEnoughResources;

                    tile.HighlightGreen(isOccupied);
                }
                else if (_tilePool[i].gameObject.activeSelf)
                {
                    _tilePool[i].DisableHighlights();
                }
            }
        }

        // Move the ghost prefab to follow the hovered tile footprint
        private void CurEntityPrefabFollow(GridCoord anchor)
        {
            if (_curRelocatedEntity == null)
                return;

            var anchorCenter = _grid.Grid.GetWorldCenter(anchor);
            var cellSize = _grid.Grid.CellSize;

            _curRelocatedEntity.transform.position = new Vector3(
                anchorCenter.x + (cellSize * (_footprint.x - 1) * 0.5f),
                anchorCenter.y,
                anchorCenter.z + (cellSize * (_footprint.y - 1) * 0.5f)
            );

            // Only apply rotation live when it's a ghost. For relocations, let rotation be explicit.
            if (_isGhostPrefab)
            {
                UpdateGhostAnchorOffsetBasedOnRotation();
            }
        }

        private void UpdateGhostAnchorOffsetBasedOnRotation()
        {
            _ghostAnchor.transform.localRotation = _footprintRotation;

            var offsetVal = 0.5f;

            var xOffsetVal = Mathf.Floor(_footprint.x/1.5f) * offsetVal;
            var yOffsetVal = Mathf.Floor(_footprint.y/1.5f) * offsetVal;

            if(_ghostAnchor.transform.localRotation.eulerAngles.y <= 0)
            {
                _ghostAnchor.transform.localPosition = new Vector3(-xOffsetVal, 0, -yOffsetVal);
            }
            else
            {
                _ghostAnchor.transform.localPosition = new Vector3(xOffsetVal, 0, -yOffsetVal);
            }
        }

        /// <summary>
        /// Retrieves pointer (mouse or touch) position and click state.
        /// </summary>
        private bool TryGetPointer(out Vector2 screenPos, out bool clickPressedThisFrame)
        {
            screenPos = default;
            clickPressedThisFrame = false;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                clickPressedThisFrame = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                return true;
            }

            if (Mouse.current != null)
            {
                screenPos = Mouse.current.position.ReadValue();

                clickPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
                return true;
            }

            return false;
        }


        private bool IsMouseHoveringUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Clamps anchor so the highlight rectangle fits inside the grid.
        /// </summary>
        private GridCoord FitAnchorToBounds(GridCoord hover, Vector2Int size)
        {
            int ax = Mathf.Clamp(hover.x, 0, _grid.Grid.Width - size.x);
            int ay = Mathf.Clamp(hover.y, 0, _grid.Grid.Height - size.y);
            return new GridCoord(ax, ay);
        }

        /// <summary>
        /// Populates a list with coordinates forming a rectangular footprint.
        /// </summary>
        private void BuildFootprintCells(GridCoord anchor, Vector2Int size, List<GridCoord> outList)
        {
            outList.Clear();

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    outList.Add(new GridCoord(anchor.x + dx, anchor.y + dy));
                }
            }
        }

        /// <summary>
        /// Ensures the pool contains enough highlight tiles.
        /// </summary>
        private void EnsurePool(int required)
        {
            if (_highlightTilePrefab == null)
                return;


            while (_tilePool.Count < required)
            {
                var tile = Instantiate(_highlightTilePrefab, transform);
                tile.gameObject.SetActive(true);
                _tilePool.Add(tile.GetComponent<GridHighlight>());
            }
        }

        /// <summary>
        /// Returns true if all target cells are unoccupied OR unbuildable
        /// </summary>
        private bool AllCellsFree(IReadOnlyList<GridCoord> cells)
        {
            foreach (var coord in cells)
            {
                var cell = _grid.Grid.Get(coord);
                var name = cell.Occupant != null ? cell.Occupant.name : "";
                Debug.Log($"Cell:{coord} | occupant: {name}");

                if (cell.Occupant != null)
                    return false;
                if (cell.Unbuildable)
                    return false;
            }
            return true;
        }

        private void CancelPlacement()
        {
            EvtOnCancelBuildingPlacing.Invoke();

            DisableHighlight();
            _highlightEnabled = false;
        }

        /// <summary>
        /// Disables all highlight tiles.
        /// </summary>
        private bool DeactivateAllTiles()
        {
            foreach (var tile in _tilePool)
            {
                tile.DisableHighlights();
            }

            return false;
        }

        #endregion
    }
}
