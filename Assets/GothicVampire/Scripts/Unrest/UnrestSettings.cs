using GothicVampire.Cycles;
using UnityEngine;

namespace GothicVampire.Unrest
{
    [CreateAssetMenu(menuName = "Unrest/Settings", order = int.MaxValue)]
    public class UnrestSettings : ScriptableObject
    {
        [SerializeField] private float _maxValue = 100f;
        [SerializeField] private CycleData _cycle;
        
        public float MaxValue => _maxValue;
        public CycleData Cycle => _cycle;
    }
}