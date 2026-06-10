using GothicVampire.Buildings;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Player.Inputs.Entity;
using GothicVampire.Roads;
using NUnit.Framework;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Grids
{
    public class RoadPlacementSystem : MonoBehaviour, IRoadService
    {
        #region Inspector
        [SerializeField] private RoadHighlighter _roadHighlighter;
        [SerializeField] private GridSystem _gridSystem;
        #endregion

        private IEntitySelectorService _entitySelector;
        private IWorldVisualEffectsService _worldVisualEffectsService;

        private RoadData _roadData;
        private bool _isPreplacingRoad;

        private Vector3 _originalOffset;
        private Vector2 _lastRoadGridSize;
        private GameObject _lastGhostPrefab;

        private RoadManager _roadManager;
        private Wallet _wallet;

        private GridCoord _pointA, _pointB;
        private RoadTier _curRoadTier;

        public void Awake()
        {
            ServiceLocator.Register<IRoadService>(this);
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            _entitySelector = ServiceLocator.Get<IEntitySelectorService>();
            _wallet = ServiceLocator.Get<World>().Player.GetService<Wallet>();
            _worldVisualEffectsService = ServiceLocator.Get<IWorldVisualEffectsService>();

            if (_roadManager == null)
            {
                _roadManager = ServiceLocator.Get<World>().Player.GetService<RoadManager>();
                _roadManager.EvtRoadRemoved.AddListener(RemoveOccupyingRoad);
            }

            if (_roadHighlighter != null)
            {
                _roadHighlighter.EvtOnFinishedRoadPlacing.AddListener(FinalizeRoadPlacement);
                _roadHighlighter.EvtOnCancelRoadPlacing.AddListener(EndRoadPlacement);
            }

            if (_wallet != null)
            {
                _wallet.EvtUpdated.AddListener(OnWalletUpdated);
            }
        }

        #region Public Method

        public List<SelectableEntity> GetStreet(SelectableEntity entity)
        {
            var result = new List<SelectableEntity>();
            var visited = new HashSet<GridCoord>();

            var road = entity.GetComponent<Road>();
            if (road == null)
                return result;

            RoadTier startTier = road.CurrentTier;
            RoadKind startKind = road.RoadKind;
            GridCoord start = road.GridPosition;

            bool startIsStraight = (startKind == RoadKind.Straight || startKind == RoadKind.DeadEnd);
            bool startIsCorner = (startKind == RoadKind.Corner);
            bool startIsIntersection = (startKind == RoadKind.IntersectionT || startKind == RoadKind.IntersectionCross);

            Queue<GridCoord> queue = new Queue<GridCoord>();
            queue.Enqueue(start);
            visited.Add(start);

            // Always include the starting tile
            result.Add(entity);

            while (queue.Count > 0)
            {
                GridCoord current = queue.Dequeue();
                bool isStart = Vector2.Equals(current, start);

                if (!_gridSystem.Grid.InBounds(current))
                    continue;

                var cell = _gridSystem.Grid.Get(current);
                if (cell == null || cell.Occupant == null)
                    continue;

                var currentRoad = cell.Occupant.GetComponent<Road>();
                if (currentRoad == null)
                    continue;

                if (currentRoad.CurrentTier != startTier)
                    continue;

                RoadKind kind = currentRoad.RoadKind;
                bool isIntersection = (kind == RoadKind.IntersectionT || kind == RoadKind.IntersectionCross);
                bool isCorner = (kind == RoadKind.Corner);
                bool isStraight = (kind == RoadKind.Straight || kind == RoadKind.DeadEnd);

                // ─────────────────────────────────────────────
                // 1. ADD TO RESULT
                // ─────────────────────────────────────────────
                if (!isStart)
                {
                    if (startIsIntersection)
                    {
                        // Intersection start: include straights and corners only
                        if (!isIntersection)
                            result.Add(currentRoad.GetComponent<SelectableEntity>());
                    }
                    else if (startIsCorner)
                    {
                        // Corner start: include straights and corners only
                        if (!isIntersection)
                            result.Add(currentRoad.GetComponent<SelectableEntity>());
                    }
                    else if (startIsStraight)
                    {
                        // Straight start: include straights + corners (NEW RULE)
                        if (!isIntersection)
                            result.Add(currentRoad.GetComponent<SelectableEntity>());
                    }
                }

                // ─────────────────────────────────────────────
                // 2. STOP SPREAD RULES
                // ─────────────────────────────────────────────

                if (!isStart)
                {
                    // Any start type stops at intersections
                    if (isIntersection)
                        continue;

                    // Corner and straight both stop spreading when they hit a corner
                    if (isCorner && !startIsIntersection)
                        continue;

                    // Intersection start also stops at corners
                    if (startIsIntersection && isCorner)
                        continue;
                }

                // ─────────────────────────────────────────────
                // 3. CONTINUE BFS
                // ─────────────────────────────────────────────
                foreach (var next in GetNeighborCoords(current))
                {
                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a List of Bool coordination [West,North,East,South]
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public List<bool> GetRoadConnections(GridCoord coord, bool forRoadHighlight)
        {
            var roadConnections = new List<bool>();
            var gridCoordinates = new List<GridCoord>();

            var west  = HasRoad(new GridCoord(coord.x, coord.y + 1), forRoadHighlight);
            var north = HasRoad(new GridCoord(coord.x + 1, coord.y), forRoadHighlight);
            var east  = HasRoad(new GridCoord(coord.x, coord.y - 1), forRoadHighlight);
            var south = HasRoad(new GridCoord(coord.x - 1, coord.y), forRoadHighlight);

            roadConnections.Add(west);
            roadConnections.Add(north);
            roadConnections.Add(east);
            roadConnections.Add(south);

            return roadConnections;
        }

        private bool HasRoad(GridCoord coord, bool isHighlightMaterial)
        {
            // For highlight previews, check the current path
            if (isHighlightMaterial)
            {
                var highlighter = _roadHighlighter;
                if (highlighter == null)
                    return false;

                List<GridCoord> previewCells = highlighter.CurrentCells;
                if (previewCells == null || previewCells.Count == 0)
                    return false;

                for (int i = 0; i < previewCells.Count; i++)
                {
                    if (previewCells[i].Equals(coord))
                        return true;
                }

                return false;
            }
            else
            {
                // For placed roads, check directly from grid
                if (!_gridSystem.Grid.InBounds(coord))
                    return false;

                var cell = _gridSystem.Grid.Get(coord);
                if (cell == null || cell.Occupant == null)
                    return false;

                return cell.OccupantEntity == Player.Inputs.Entity.EntityType.Road;
            }
        }


        public void PlaceRoad(RoadData roadData)
        {
            if (!_roadManager.CanPurchase(roadData)) return;

            if(_entitySelector != null)
            {
                _entitySelector.UnselectCurrentEntity();
            }
            _curRoadTier = roadData.Tiers[0];
            _roadData = roadData;
            PrePlaceRoad(_roadData.GridSize, _roadData.Ghost);
        }

        public Vector3 GridCoordToWorldPosition(GridCoord gridCoord)
        {
            return _gridSystem.Grid.GetWorldCenter(gridCoord);
        }

        public bool IsPreplacingRoad()
        {
            return _isPreplacingRoad;
        }

        public void RemoveOccupyingRoad(Road road)
        {
            var cell = _gridSystem.Grid.Get(road.GridPosition);

            cell.OccupantEntity = EntityType.None;
            cell.Occupant = null;
            cell.Walkable = true;
        }

        #endregion

        #region Private Method

        private List<GridCoord> GetNeighborCoords(GridCoord c)
        {
            return new List<GridCoord>
            {
                new GridCoord(c.x, c.y + 1), // North
                new GridCoord(c.x + 1, c.y), // East
                new GridCoord(c.x, c.y - 1), // South
                new GridCoord(c.x - 1, c.y)  // West
            };
        }

        private void PrePlaceRoad(Vector2 roadGridSize, GameObject ghostPrefab)
        {
            _isPreplacingRoad = true;

            _lastRoadGridSize = roadGridSize;
            _lastGhostPrefab = ghostPrefab;

            _roadHighlighter.SetPrePlaceRoad(ghostPrefab);
            _roadHighlighter.SetHighlightGridSize(roadGridSize);
            _worldVisualEffectsService.SetVisualModelsToGhost();
        }

        private void FinalizeRoadPlacement(GridCoord[] gridCoords)
        {
            foreach (var gridCoord in gridCoords)
            {
                var newRoad =  _roadManager.Build(_roadData, gridCoord, Quaternion.identity);
                newRoad.gameObject.name = $"Road {gridCoord}";

                _roadHighlighter.SetCoordOccupant(newRoad.gameObject, gridCoord);
            }

            _worldVisualEffectsService.SetVisualModelsToNormal();
            _roadManager.UpdateRoads();

            EndRoadPlacement();
        }

        private void EndRoadPlacement()
        {
            _roadHighlighter.DeleteCurGhostPrefab();
            _roadHighlighter.DisableHighlight();
            _worldVisualEffectsService.SetVisualModelsToNormal();
            Reset();
        }

        private void OnWalletUpdated(Wallet wallet)
        {
            if (_roadData == null) return;

            var canPurchase = _roadManager.CanPurchase(_roadData);
            _roadHighlighter.NotEnoughResources(!canPurchase);
        }

        private void Reset()
        {
            _pointA = Vector2.zero;
            _pointB = Vector2.zero;
            _isPreplacingRoad = false;

            _roadData = null;
            _curRoadTier = null;
        }

        #endregion

        #region IRoadService

        List<SelectableEntity> IRoadService.GetStreet(SelectableEntity entity) => GetStreet(entity);
        Vector3 IRoadService.GridToWorldPosition(GridCoord gridPos) => GridCoordToWorldPosition(gridPos);
        void IRoadService.PlaceRoad(RoadData newRoad) => PlaceRoad(newRoad);
        RoadTier IRoadService.GetPreplacedRoadTier => _curRoadTier;
        RoadHighlighter IRoadService.RoadHighlighter => _roadHighlighter;
        Grid2D<GridCell> IRoadService.GridCell => _gridSystem.Grid;
        bool IRoadService.IsPreplacingRoad() => IsPreplacingRoad();

        List<bool> IRoadService.GetRoadConnections(GridCoord coord, bool forRoadHighlights) => GetRoadConnections(coord, forRoadHighlights);

        #endregion
    }
}
