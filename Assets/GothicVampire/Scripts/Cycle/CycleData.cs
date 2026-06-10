using UnityEngine;

namespace GothicVampire.Cycles
{
    /// <summary>
    /// Defines a cycle resolved by world
    /// </summary>
    [CreateAssetMenu(menuName = "Cycle/Cycle", order = 0)]
    public sealed class CycleData : ScriptableObject
    {
        [SerializeField] private float _duration = 1f;

        public string Id => name;
        public float Duration => _duration;
    }
}