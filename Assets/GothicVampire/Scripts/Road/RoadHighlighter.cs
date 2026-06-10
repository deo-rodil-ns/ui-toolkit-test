using GothicVampire.Roads;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using GothicVampire.Player.Inputs.Entity;
using System.Linq;

namespace GothicVampire.Grids
{
    public class RoadHighlighter : MonoBehaviour
    {
        #region Events

        public UnityEvent<GridCoord[]> EvtOnFinishedRoadPlacing { get; } = new();
        public UnityEvent EvtOnCancelRoadPlacing { get; } = new();

        private UnityEvent<GridCoord> _evtOnHighlightUpdated = new UnityEvent<GridCoord>();

        #endregion

        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Transform _highlightContainer;
        [Header("Highlight")]
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private Transform _roadHighlightTilePrefab;
        [Header("State")]
        [SerializeField] private GameObject _curRelocatedEntity;
        [SerializeField] private bool _highlightEnabled = false;
        [SerializeField] private bool _isGhostPrefab = false;

        #endregion

        #region Public Fields

        public List<GridCoord> CurrentCells => _currentCells;

        #endregion
        
        #region Private Fields

        private GridSystem _grid;

        private readonly List<RoadHighlight> _tilePool = new List<RoadHighlight>();
        private readonly List<GridCoord> _currentCells = new();
        private Vector2Int _footprint = new(1, 1); // width (x) by height (y) in grid cells
        private Quaternion _footprintRotation = Quaternion.identity;


        private GridCoord _roadPointA;
        private bool _roadPointAPlaced = false;
        private GameObject _ghostAnchor;
        private bool _notEnoughResources = false;
        private IRoadService _roadService;
        private Camera _mainCam;
        #endregion

        private void Start()
        {
            _roadService = ServiceLocator.Get<IRoadService>();
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

            // Cancel
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                EndPlacement();
                return;
            }

            var ray = _mainCam.ScreenPointToRay(screenPos);

            if (!_grid.TryGetCoordFromRay(ray, out var hoverCoord, out _))
            {
                DeactivateAllTiles();
                return;
            }

            UpdateRoadHighlights(hoverCoord, clickPressedThisFrame);
        }

        #region Public Methods

        public void SetPrePlaceRoad(GameObject newPrefab)
        {
            _roadPointA = Vector2.zero;
            _roadPointAPlaced = false;

            _curRelocatedEntity = Instantiate(newPrefab, transform);
            _isGhostPrefab = true;
            _ghostAnchor = _curRelocatedEntity.transform.GetChild(0).gameObject;
            UpdateGhostAnchorOffsetBasedOnRotation();
            UpdateCurrentTilePools();

            EnableHighlight();
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

        public void DeleteCurGhostPrefab()
        {
            if (_isGhostPrefab)
            {
                Destroy(_curRelocatedEntity);
            }

        }

        public void SetOccupant(GameObject gameobject)
        {
            foreach (var coord in _currentCells)
            {
                var cell = _grid.Grid.Get(coord);
                cell.Occupant = gameobject;
                cell.OccupantEntity = EntityType.Road;
                cell.Walkable = true;
            }
        }

        public void SetCoordOccupant(GameObject gameObject, GridCoord coord)
        {
            var cell = _grid.Grid.Get(coord);
            gameObject.name = $"Road {coord}";
            
            cell.Occupant = gameObject;
            cell.OccupantEntity = EntityType.Road;
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

        public void NotEnoughResources(bool newState)
        {
            _notEnoughResources = newState;
        }

        #endregion

        #region Private Helpers

        private void UpdateRoadHighlights(GridCoord hoveredGridPoint, bool clickPressedThisFrame)
        {
            CurEntityPrefabFollow(hoveredGridPoint);

            if (!_roadPointAPlaced)
            {
                _currentCells.Clear();
                _currentCells.Add(hoveredGridPoint);
                EnsurePool(1);
            }
            else
            {
                BuildRoadGridCells(_roadPointA, hoveredGridPoint, _currentCells);
                EnsurePool(_currentCells.Count);
            }

            // Position active highlight tiles
            PositionHighlightTiles();

            // Attempt placement if user clicked
            if (clickPressedThisFrame && !IsMouseHoveringUi() && AllCellsFree(_currentCells))
            {
                // Point A and Point B is already setup.
                if (_roadPointAPlaced)
                {
                    var filteredCoordCells = FilteredRoadCells(_currentCells);
                    Debug.Log($"Road Count:{filteredCoordCells.Count}");

                    EvtOnFinishedRoadPlacing.Invoke(filteredCoordCells.ToArray());
                }
                else
                {
                    _roadPointAPlaced = true;
                    _roadPointA = new Vector2(hoveredGridPoint.x, hoveredGridPoint.y);
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
        }

        private List<GridCoord> FilteredRoadCells(List<GridCoord> cells)
        {
            var coords = new List<GridCoord>(cells);

            foreach (var coord in cells)
            {
                var cell = _grid.Grid.Get(coord);
                if (!IsCellOccupied(cell) && cell.OccupantEntity == EntityType.Road)
                {
                    Debug.Log($"Occupied");
                    coords.Remove(coord);
                }
            }

            return coords;
        }

        private void BuildRoadGridCells(GridCoord pointA, GridCoord pointB, List<GridCoord> outList)
        {
            outList.Clear();

            if (_grid == null || _grid.Grid == null)
                return;

            // Quick exits
            if (!_grid.Grid.InBounds(pointA) || !_grid.Grid.InBounds(pointB))
                return;

            // Convert blocked cells into fast lookup
            var blocked = new HashSet<GridCoord>(_grid.GetRoadBlockedCells());

            // BFS setup
            Queue<GridCoord> frontier = new Queue<GridCoord>();
            Dictionary<GridCoord, GridCoord> cameFrom = new Dictionary<GridCoord, GridCoord>();

            frontier.Enqueue(pointA);
            cameFrom[pointA] = pointA;

            // Directions: N, S, E, W
            var directions = new[]
            {
                new GridCoord(1, 0),
                new GridCoord(-1, 0),
                new GridCoord(0, 1),
                new GridCoord(0, -1)
            };

            bool found = false;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(pointB))
                {
                    found = true;
                    break;
                }

                foreach (var dir in directions)
                {
                    var next = new GridCoord(current.x + dir.x, current.y + dir.y);

                    if (!_grid.Grid.InBounds(next)) continue;
                    if (blocked.Contains(next)) continue;
                    if (cameFrom.ContainsKey(next)) continue;

                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }

            if (!found)
            {
                // No valid path found; return the start only
                outList.Add(pointA);
                return;
            }

            // Reconstruct path from B → A
            var path = new List<GridCoord>();
            var cur = pointB;

            while (!cur.Equals(pointA))
            {
                path.Add(cur);
                cur = cameFrom[cur];
            }

            path.Add(pointA);
            path.Reverse();

            // Copy to outList
            outList.AddRange(path);
        }


        /// <summary>
        /// Ensures the pool contains enough highlight tiles.
        /// </summary>
        private void EnsurePool(int required)
        {
            if (_roadHighlightTilePrefab == null)
                return;


            while (_tilePool.Count < required)
            {
                CreateNewTileHighlight();
                _tilePool[_tilePool.Count - 1].RoadMaterialSystem.Initialize(_roadService.GetPreplacedRoadTier);
            }
        }

        private void CreateNewTileHighlight()
        {
            var tile = Instantiate(_roadHighlightTilePrefab, _highlightContainer);
            tile.gameObject.SetActive(true);

            var roadHighlight = tile.GetComponent<RoadHighlight>();
            _evtOnHighlightUpdated.AddListener(roadHighlight.RoadMaterialSystem.UpdateRoadMaterial);

            _tilePool.Add(roadHighlight);
        }

        private void UpdateCurrentTilePools()
        {
            foreach (var tile in _tilePool)
            {
                tile.RoadMaterialSystem.Initialize(_roadService.GetPreplacedRoadTier);
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
                    tile.position = new Vector3(center.x, 0.01f, center.z);

                    // Remove Later
                    tile.name = i.ToString();

                    var isOccupied = IsCellOccupied(cell);

                    tile.RoadMaterialSystem.UpdateRoadMaterial(coord);
                    tile.HighlightGreen(isOccupied);
                }
                else if (_tilePool[i].gameObject.activeSelf)
                {
                    _tilePool[i].DisableHighlights();
                }
            }
        }

        private bool IsCellOccupied(GridCell cell)
        {
            if(cell.Occupant != null)
            {
                return cell.OccupantEntity != EntityType.Road;
            }

            return cell.Unbuildable || _notEnoughResources;
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

        private void EndPlacement()
        {
            EvtOnCancelRoadPlacing.Invoke();

            _roadPointA = Vector2.zero;
            _roadPointAPlaced = false;
        }

        private void UpdateGhostAnchorOffsetBasedOnRotation()
        {
            _ghostAnchor.transform.localRotation = _footprintRotation;

            var offsetVal = 0.5f;

            var xOffsetVal = Mathf.Floor(_footprint.x / 1.5f) * offsetVal;
            var yOffsetVal = Mathf.Floor(_footprint.y / 1.5f) * offsetVal;

            if (_ghostAnchor.transform.localRotation.eulerAngles.y <= 0)
            {
                _ghostAnchor.transform.localPosition = new Vector3(-xOffsetVal, 0, -yOffsetVal);
            }
            else
            {
                _ghostAnchor.transform.localPosition = new Vector3(xOffsetVal, 0, -yOffsetVal);
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
                if (IsCellOccupied(cell))
                    return false;
            }
            return true;
        }

        private bool IsMouseHoveringUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
            else
                return false;
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
