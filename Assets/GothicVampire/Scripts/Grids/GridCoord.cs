using UnityEngine;

namespace GothicVampire.Grids
{
    [System.Serializable]
    public struct GridCoord
    {
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        
        public int x { get => _x; set => _x = value; }
        public int y { get => _y; set => _y = value; }

        public GridCoord(int x, int y)
        {
            _x = x; 
            _y = y;
        }

        public GridCoord(Vector2 pos)
        {
            _x = (int)pos.x;
            _y = (int)pos.y;
        }

        public bool Equals(GridCoord other)
        {
            return _x == other._x && _y == other._y;
        }

        public static implicit operator GridCoord(Vector2 pos) => new(pos);
        public static implicit operator Vector2(GridCoord coord) => new(coord.x, coord.y);
        
        public override string ToString() => $"({x},{y})";
    }
}
