// Grid2D.cs
using System;
using UnityEngine;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Generic 2D grid (XZ plane) with utility conversions and change notifications.
    /// Plain C# class (POCO): no Unity lifecycle dependencies.
    /// </summary>
    public class Grid2D<T>
    {
        #region Fields

        private readonly T[,] _cells;

        #endregion

        #region Events

        /// <summary>
        /// Raised whenever a cell value changes.
        /// </summary>
        public event Action<GridCoord, T> EvtCellChanged;

        #endregion

        #region Properties

        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public Vector3 Origin { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a grid and initializes each cell via <paramref name="createFn"/>.
        /// </summary>
        public Grid2D(
            int width,
            int height,
            float cellSize,
            Vector3 origin,
            Func<Grid2D<T>, GridCoord, T> createFn)
        {
            if (createFn == null)
                throw new ArgumentNullException(nameof(createFn));

            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            CellSize = Mathf.Max(0.01f, cellSize);
            Origin = origin;

            _cells = new T[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var coord = new GridCoord(x, y);
                    _cells[x, y] = createFn(this, coord);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if <paramref name="coord"/> is inside the grid bounds.
        /// </summary>
        public bool InBounds(GridCoord coord) => coord.x >= 0 && coord.y >= 0 && coord.x < Width && coord.y < Height;

        /// <summary>
        /// Converts a grid coordinate to the world-space center of that cell.
        /// </summary>
        public Vector3 GetWorldCenter(GridCoord coord)
        {
            EnsureInBounds(coord);
            return Origin + new Vector3((coord.x + 0.5f) * CellSize, 0f, (coord.y + 0.5f) * CellSize);
        }

        /// <summary>
        /// Returns the world position for a given grid coordinate and rotation,
        /// optionally applying an offset of one cell forward in the facing direction.
        /// </summary>
        public Vector3 GetWorldCenterRotated(GridCoord gridPos, Quaternion rotation, float offsetMultiplier = 1f)
        {
            EnsureInBounds(gridPos);
            Vector3 worldPos = GetWorldCenter(gridPos);

            // Normalize Y rotation
            float yRot = rotation.eulerAngles.y;
            if (yRot > 180f) yRot -= 360f;

            Vector3 offset = Vector3.zero;
            if (Mathf.Approximately(yRot, 0f))
                offset = new Vector3(0f, 0f, 1f);
            else if (Mathf.Approximately(yRot, 90f))
                offset = new Vector3(1f, 0f, 0f);
            else if (Mathf.Approximately(yRot, 180f) || Mathf.Approximately(yRot, -180f))
                offset = new Vector3(0f, 0f, -1f);
            else if (Mathf.Approximately(yRot, -90f))
                offset = new Vector3(-1f, 0f, 0f);

            // Apply offset scaled by cell size
            worldPos += offset * CellSize * offsetMultiplier;
            return worldPos;
        }


        /// <summary>
        /// Converts a grid coordinate to the world-space minimum (south-west) corner of that cell.
        /// </summary>
        public Vector3 GetWorldMin(GridCoord coord)
        {
            EnsureInBounds(coord);
            return Origin + new Vector3(coord.x * CellSize, 0f, coord.y * CellSize);
        }

        /// <summary>
        /// Converts a world position to the containing grid coordinate (may be out of bounds).
        /// </summary>
        public GridCoord GetCoordFromWorld(Vector3 worldPos)
        {
            var local = worldPos - Origin;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.z / CellSize);
            return new GridCoord(x, y);
        }

        /// <summary>
        /// Gets the value at <paramref name="coord"/>. Throws if out of bounds.
        /// </summary>
        public T Get(GridCoord coord)
        {
            EnsureInBounds(coord);
            return _cells[coord.x, coord.y];
        }

        /// <summary>
        /// Tries to get the value at <paramref name="coord"/> without throwing.
        /// </summary>
        public bool TryGet(GridCoord coord, out T value)
        {
            if (InBounds(coord))
            {
                value = _cells[coord.x, coord.y];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Sets the value at <paramref name="coord"/>. Throws if out of bounds.
        /// </summary>
        public void Set(GridCoord coord, T value)
        {
            EnsureInBounds(coord);
            _cells[coord.x, coord.y] = value;
            EvtCellChanged?.Invoke(coord, value);
        }

        /// <summary>
        /// Tries to set the value at <paramref name="coord"/> without throwing.
        /// </summary>
        public bool TrySet(GridCoord coord, T value)
        {
            if (!InBounds(coord))
                return false;

            _cells[coord.x, coord.y] = value;
            EvtCellChanged?.Invoke(coord, value);
            return true;
        }

        /// <summary>
        /// Iterates over all cells (row-major).
        /// </summary>
        public void ForEach(Action<GridCoord, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    action(new GridCoord(x, y), _cells[x, y]);
                }
            }
        }

        #endregion

        #region Private Methods

        private void EnsureInBounds(GridCoord coord)
        {
            if (!InBounds(coord))
                Debug.LogError($"Coord {coord} is out of bounds [0..{Width - 1}, 0..{Height - 1}].");
        }

        #endregion
    }
}
