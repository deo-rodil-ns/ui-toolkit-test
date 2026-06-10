using System;

namespace GothicVampire.Grids
{
    public enum EdgeDirection
    {
        North,
        South,
        East,
        West
    }

    public readonly struct EdgeNode : IEquatable<EdgeNode>
    {
        public readonly GridCoord Cell;
        public readonly EdgeDirection Dir;

        public EdgeNode(GridCoord cell, EdgeDirection dir)
        {
            Cell = cell;
            Dir = dir;
        }

        public bool Equals(EdgeNode other)
        {
            return Cell.Equals(other.Cell) && Dir == other.Dir;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeNode other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Cell.GetHashCode() * 397) ^ (int)Dir;
            }
        }

        public static bool operator ==(EdgeNode a, EdgeNode b) => a.Equals(b);
        public static bool operator !=(EdgeNode a, EdgeNode b) => !a.Equals(b);

        public override string ToString() => $"{Cell} @ {Dir}";
    }
}

