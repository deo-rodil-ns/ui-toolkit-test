namespace GothicVampire.Grids
{
    public class HybridNode
    {
        public object Node; // GridCoord or EdgeNode
        public HybridNode Parent;
        public float G;
        public float H;
        public float F => G + H;
    }

}
