using System;

namespace GothicVampire.Grids
{
    public class HybridPathNode
    {
        public object Node;     // GridCoord OR EdgeNode
        public HybridPathNode Parent;
        public float G;         // Cost from start
        public float H;         // Heuristic to goal
        public float F => G + H;
    }
}
