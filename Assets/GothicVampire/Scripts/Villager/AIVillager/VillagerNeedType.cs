using System;
using UnityEngine;

namespace GothicVampire.Villagers
{
    [Serializable]
    public class VillagerNeedType
    {
        [Header("Reference")]
        [SerializeField] private VillagerNeedData _type;

        [Header("Configuration")]
        [SerializeField] private float _maxValue = 100;
        [SerializeField] private float _rechargeSpeed;
        [SerializeField] private float _decayRate;

        [Header("State")]
        [SerializeField] private float _value;

        public bool IsRecharging { get; set; }
        public VillagerNeedData Type => _type;
        public float MaxValue => _maxValue;
        public float RechargeSpeed => _rechargeSpeed;
        public float DecayRate => _decayRate;

        public float Value
        {
            get => _value;
            private set => _value = Mathf.Clamp(value, 0f, _maxValue);
        }

        public void Tick(float deltaTime)
        {
            Value += (IsRecharging ? _rechargeSpeed : -_decayRate) * deltaTime;
        }
    }
}
