// GridSystem.cs
using Sylpheed.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Handles creation and world-space mapping of a 2D grid.
    /// Acts as the entry point for grid-based systems within the scene.
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Grid Settings")]
        [SerializeField] private int _width = 20;
        [SerializeField] private int _height = 20;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector3 _origin = Vector3.zero;

        [Header("Interaction Settings")]
        [Tooltip("Set this to your ground or terrain layer.")]
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private bool _drawGizmos = true;

        [Tooltip("Any collider on this layer will block building (marked unbuildable).")]
        [SerializeField] private LayerMask _unbuildableMask;
        private List<GridCell> _unbuildableGrids = new List<GridCell>();

        #endregion

        #region Properties

        /// <summary>
        /// The generated 2D grid of <see cref="GridCell"/>s.
        /// </summary>
        public Grid2D<GridCell> Grid { get; private set; }

        public IReadOnlyList<GridCell> UnbuildableGrids => _unbuildableGrids;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Grid = new Grid2D<GridCell>(
                _width,
                _height,
                _cellSize,
                _origin,
                (grid, coord) => new GridCell
                {
                    Walkable = true,
                    Occupant = null,
                    WorldCenter = grid.GetWorldCenter(coord)
                }
            );

            ScanUnbuildables();

            ServiceLocator.Register<GridSystem>(this);
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;

            Gizmos.color = Color.gray;

            // Draw outer boundary
            Vector3 gridSize = new Vector3(_width * _cellSize, 0f, _height * _cellSize);
            Gizmos.DrawWireCube(_origin + new Vector3(gridSize.x * 0.5f, 0f, gridSize.z * 0.5f), gridSize);

            // Use runtime values if available
            int w = Application.isPlaying ? Grid.Width : _width;
            int h = Application.isPlaying ? Grid.Height : _height;
            float cs = Application.isPlaying ? Grid.CellSize : _cellSize;
            Vector3 o = Application.isPlaying ? Grid.Origin : _origin;

            // Draw grid lines
            for (int x = 0; x <= w; x++)
            {
                Vector3 start = o + new Vector3(x * cs, 0f, 0f);
                Vector3 end = o + new Vector3(x * cs, 0f, h * cs);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= h; y++)
            {
                Vector3 start = o + new Vector3(0f, 0f, y * cs);
                Vector3 end = o + new Vector3(w * cs, 0f, y * cs);
                Gizmos.DrawLine(start, end);
            }

            if (Application.isPlaying && Grid != null)
            {
                foreach (var coord in new GridCoordEnumerable(Grid.Width, Grid.Height))
                {
                    var cell = Grid.Get(coord);
                    if (cell.Unbuildable)
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                        Gizmos.DrawCube(cell.WorldCenter, Vector3.one * _cellSize * 0.9f);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public bool IsCellOccupied(GridCoord coord)
        {
            return Grid.Get(coord).Occupant != null;
        }

        public bool IsCellOccupiedByStructures(GridCoord coord)
        {
            var cell = Grid.Get(coord);
            return cell != null && cell.OccupantEntity != Player.Inputs.Entity.EntityType.Road;
        }

        /// <summary>
        /// Returns a list of all grid coordinates that are either occupied or unbuildable.
        /// </summary>
        public List<GridCoord> GetBlockedCells()
        {
            var blockedCoords = new List<GridCoord>();

            if (Grid == null)
                return blockedCoords;

            foreach (var coord in new GridCoordEnumerable(Grid.Width, Grid.Height))
            {
                var cell = Grid.Get(coord);
                if (cell == null)
                    continue;

                // Check for either Occupant or Unbuildable
                if (cell.Occupant != null || cell.Unbuildable)
                {
                    blockedCoords.Add(coord);
                }
            }

            return blockedCoords;
        }

        /// <summary>
        /// Returns a list of all grid coordinates that aren't a road AND occupied or unbuildable.
        /// </summary>
        public List<GridCoord> GetRoadBlockedCells()
        {
            var blockedCoords = new List<GridCoord>();

            if (Grid == null)
                return blockedCoords;

            foreach (var coord in new GridCoordEnumerable(Grid.Width, Grid.Height))
            {
                var cell = Grid.Get(coord);
                if (cell == null)
                    continue;

                // Check for either Occupant or Unbuildable
                if (cell.OccupantEntity != Player.Inputs.Entity.EntityType.Road && (cell.Occupant != null || cell.Unbuildable))
                {
                    blockedCoords.Add(coord);
                }
            }

            return blockedCoords;
        }

        /// <summary>
        /// Attempts to convert a ray into a valid grid coordinate on the ground plane.
        /// </summary>
        /// <param name="ray">Input world-space ray.</param>
        /// <param name="coord">Resulting grid coordinate, if valid.</param>
        /// <param name="worldCenter">Center of the grid cell, if valid.</param>
        /// <returns>True if the ray hits a valid grid coordinate.</returns>
        public bool TryGetCoordFromRay(Ray ray, out GridCoord coord, out Vector3 worldCenter)
        {
            coord = default;
            worldCenter = default;

            if (Physics.Raycast(ray, out var hit, 5000f, _groundMask))
            {
                var c = Grid.GetCoordFromWorld(hit.point);
                if (Grid.InBounds(c))
                {
                    coord = c;
                    worldCenter = Grid.GetWorldCenter(c);
                    return true;
                }
            }

            return false;
        }

        public Vector3 GetWorldPosition(EdgeNode node)
        {
            Vector3 center = Grid.GetWorldCenter(node.Cell);
            float offset = Grid.CellSize * 0.4f; // distance from center toward edge

            return node.Dir switch
            {
                EdgeDirection.North => center + new Vector3(0, 0, offset),
                EdgeDirection.South => center + new Vector3(0, 0, -offset),
                EdgeDirection.East => center + new Vector3(offset, 0, 0),
                EdgeDirection.West => center + new Vector3(-offset, 0, 0),
                _ => center
            };
        }

        public Vector3 GetWorldPosition(GridCoord coord)
        {
            return Grid.GetWorldCenter(coord);
        }

        public EdgeNode GetNearestEdgeNodeToWorld(Vector3 worldPos)
        {
            var coord = Grid.GetCoordFromWorld(worldPos);
            if (!Grid.InBounds(coord))
                coord = new GridCoord(
                    Mathf.Clamp(coord.x, 0, Grid.Width - 1),
                    Mathf.Clamp(coord.y, 0, Grid.Height - 1)
                );

            Vector3 center = Grid.GetWorldCenter(coord);
            Vector3 offset = worldPos - center;

            // Decide edge by which axis we're more offset on
            if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
            {
                // More offset in X => East/West
                var dir = offset.x >= 0 ? EdgeDirection.East : EdgeDirection.West;
                return new EdgeNode(coord, dir);
            }
            else
            {
                // More offset in Z => North/South
                var dir = offset.z >= 0 ? EdgeDirection.North : EdgeDirection.South;
                return new EdgeNode(coord, dir);
            }
        }

        public bool IsStraightPathClear(GridCoord a, GridCoord b)
        {
            // Only allow perfectly horizontal or vertical straight paths
            if (a.x != b.x && a.y != b.y)
                return false;

            // Horizontal path
            if (a.y == b.y)
            {
                int start = Mathf.Min(a.x, b.x);
                int end = Mathf.Max(a.x, b.x);

                for (int x = start; x <= end; x++)
                {
                    var c = new GridCoord(x, a.y);
                    var cell = Grid.Get(c);

                    if (cell == null || cell.Unbuildable || cell.Occupant != null)
                        return false; // blocked
                }

                return true; // clear
            }

            // Vertical path
            if (a.x == b.x)
            {
                int start = Mathf.Min(a.y, b.y);
                int end = Mathf.Max(a.y, b.y);

                for (int y = start; y <= end; y++)
                {
                    var c = new GridCoord(a.x, y);
                    var cell = Grid.Get(c);

                    if (cell == null || cell.Unbuildable || cell.Occupant != null)
                        return false; // blocked
                }

                return true; // clear
            }

            return false;
        }

        public EdgeNode GetDefaultEdgeNodeForCell(GridCoord cell)
        {
            // For now, just use North (you can customize this per-building later)
            return new EdgeNode(cell, EdgeDirection.North);
        }

        public List<object> GetHybridNeighbors(object node)
        {
            List<object> result = new();

            // CENTER NOD
            if (node is GridCoord center)
            {
                // A) Center → Center neighbors
                foreach (var n in GetNeighbors(center))
                {
                    var c = Grid.Get(n);

                    // Only walk to center if empty cell
                    if (c != null && !c.Unbuildable && c.Occupant == null)
                        result.Add(n);
                }

                // B) Center → Edge nodes (inside same tile)
                result.Add(new EdgeNode(center, EdgeDirection.North));
                result.Add(new EdgeNode(center, EdgeDirection.South));
                result.Add(new EdgeNode(center, EdgeDirection.East));
                result.Add(new EdgeNode(center, EdgeDirection.West));

                return result;
            }

            // EDGE NODE
            if (node is EdgeNode edge)
            {
                // A) Edge → Edge neighbors
                foreach (var e in GetEdgeNeighbors(edge))
                    result.Add(e);

                // B) Edge → Center if walkable
                if (HybridPathfinder.IsWalkable(this, edge.Cell))
                    result.Add(edge.Cell);

                return result;
            }

            return result;
        }

        /// <summary>
        /// Returns valid 4-direction neighbors (N, S, E, W) for pathfinding.
        /// Does not check walkability — the pathfinder should handle that.
        /// </summary>
        public List<GridCoord> GetNeighbors(GridCoord coord)
        {
            List<GridCoord> neighbors = new();

            // Up (North)
            var up = new GridCoord(coord.x, coord.y + 1);
            if (Grid.InBounds(up)) neighbors.Add(up);

            // Down (South)
            var down = new GridCoord(coord.x, coord.y - 1);
            if (Grid.InBounds(down)) neighbors.Add(down);

            // Right (East)
            var right = new GridCoord(coord.x + 1, coord.y);
            if (Grid.InBounds(right)) neighbors.Add(right);

            // Left (West)
            var left = new GridCoord(coord.x - 1, coord.y);
            if (Grid.InBounds(left)) neighbors.Add(left);

            return neighbors;
        }

        public List<EdgeNode> GetEdgeNeighbors(EdgeNode node)
        {
            List<EdgeNode> result = new();

            GridCoord c = node.Cell;

            switch (node.Dir)
            {
                case EdgeDirection.North:
                    // Connect within the same cell
                    result.Add(new EdgeNode(c, EdgeDirection.East));
                    result.Add(new EdgeNode(c, EdgeDirection.West));

                    // Connect to the cell above (if walkable)
                    var northCell = new GridCoord(c.x, c.y + 1);
                    if (Grid.InBounds(northCell) && !Grid.Get(northCell).Unbuildable)
                        result.Add(new EdgeNode(northCell, EdgeDirection.South));
                    break;

                case EdgeDirection.South:
                    result.Add(new EdgeNode(c, EdgeDirection.East));
                    result.Add(new EdgeNode(c, EdgeDirection.West));

                    var southCell = new GridCoord(c.x, c.y - 1);
                    if (Grid.InBounds(southCell) && !Grid.Get(southCell).Unbuildable)
                        result.Add(new EdgeNode(southCell, EdgeDirection.North));
                    break;

                case EdgeDirection.East:
                    result.Add(new EdgeNode(c, EdgeDirection.North));
                    result.Add(new EdgeNode(c, EdgeDirection.South));

                    var eastCell = new GridCoord(c.x + 1, c.y);
                    if (Grid.InBounds(eastCell) && !Grid.Get(eastCell).Unbuildable)
                        result.Add(new EdgeNode(eastCell, EdgeDirection.West));
                    break;

                case EdgeDirection.West:
                    result.Add(new EdgeNode(c, EdgeDirection.North));
                    result.Add(new EdgeNode(c, EdgeDirection.South));

                    var westCell = new GridCoord(c.x - 1, c.y);
                    if (Grid.InBounds(westCell) && !Grid.Get(westCell).Unbuildable)
                        result.Add(new EdgeNode(westCell, EdgeDirection.East));
                    break;
            }

            return result;
        }

        public bool IsWalkable(GridCoord coord)
        {
            if(!Grid.InBounds(coord)) return false;

            var cell = Grid.Get(coord);

            return cell != null && cell.Walkable;
        }

        public void ComputePenaltyFields()
        {
            foreach (var coord in new GridCoordEnumerable(Grid.Width, Grid.Height))
            {
                var cell = Grid.Get(coord);
                cell.AdjacentObstacleCount = 0;

                foreach (var n in GetNeighbors(coord))
                {
                    var neighbor = Grid.Get(n);
                    if (neighbor == null) continue;

                    if (neighbor.Occupant != null || neighbor.Unbuildable)
                        cell.AdjacentObstacleCount++;
                }
            }
        }

        public GridCoord GetCoordFromDistance(GridCoord coord, int distance)
        {
            // All possible tiles exactly 'distance' steps away (Manhattan ring)
            List<GridCoord> candidates = new();

            for (int dx = -distance; dx <= distance; dx++)
            {
                int dy1 = distance - Mathf.Abs(dx);
                int dy2 = -dy1;

                candidates.Add(new GridCoord(coord.x + dx, coord.y + dy1));
                if (dy1 != 0)
                    candidates.Add(new GridCoord(coord.x + dx, coord.y + dy2));
            }

            // Filter in-bounds
            candidates.RemoveAll(c => !Grid.InBounds(c));

            // Pick any (or you can return the whole list)
            if (candidates.Count == 0)
                return coord;

            return candidates[Random.Range(0, candidates.Count)];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Scans for all colliders in the Unbuildables layer and marks affected grid cells.
        /// </summary>
        private void ScanUnbuildables()
        {
            if (Grid == null) return;

            var colliders = Physics.OverlapBox(
                _origin + new Vector3(_width * _cellSize / 2f, 0f, _height * _cellSize / 2f),
                new Vector3(_width * _cellSize / 2f, 50f, _height * _cellSize / 2f),
                Quaternion.identity,
                _unbuildableMask
            );

            foreach (var col in colliders)
            {
                // Check which cell this collider overlaps
                Bounds bounds = col.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;

                var minCoord = Grid.GetCoordFromWorld(min);
                var maxCoord = Grid.GetCoordFromWorld(max);

                for (int x = minCoord.x; x <= maxCoord.x; x++)
                {
                    for (int y = minCoord.y; y <= maxCoord.y; y++)
                    {
                        var coord = new GridCoord(x, y);
                        if (!Grid.InBounds(coord)) continue;

                        var cell = Grid.Get(coord);
                        cell.Unbuildable = true;
                        cell.Walkable = false;
                        cell.Occupant = col.gameObject;

                        _unbuildableGrids.Add(cell);
                    }
                }
            }

            ComputePenaltyFields();
        }

        #endregion
    }
}
