// GridCoordEnumerable.cs
using System.Collections;
using System.Collections.Generic;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Helper struct for iterating through all grid coordinates in a Grid2D.
    /// Usage:
    /// foreach (var coord in new GridCoordEnumerable(width, height)) { ... }
    /// </summary>
    public readonly struct GridCoordEnumerable : IEnumerable<GridCoord>
    {
        private readonly int _width;
        private readonly int _height;

        public GridCoordEnumerable(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public IEnumerator<GridCoord> GetEnumerator()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    yield return new GridCoord(x, y);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
