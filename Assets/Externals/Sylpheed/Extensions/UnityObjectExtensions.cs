namespace Sylpheed.Extensions
{
    public static class UnityObjectExtensions
    {
        public static bool IsNullOrDestroyed(this UnityEngine.Object obj)
        {
            if (ReferenceEquals(obj, null)) return true;
            return obj == null;
        }

        public static bool IsAlive(this UnityEngine.Object obj)
        {
            return !IsNullOrDestroyed(obj);
        }
    }
}