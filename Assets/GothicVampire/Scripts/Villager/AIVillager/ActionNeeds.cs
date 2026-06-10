using System;
using System.Linq;
using UnityEngine;
using GothicVampire.Villagers.Actions;

namespace GothicVampire.Villagers
{
    [Serializable]
    public class ActionNeeds
    {
        [Header("Reference")]
        [SerializeField] private VillagerNeedData _needRequired;

        [Header("Evaluation Configuration")]
        [SerializeField] private float threshold;
        [SerializeField] private float pointPerPointsBelowThreshold = 1f;

        private VillagerBrain _brain;
        private VillagerAction _action;

        public VillagerNeedData NeedRequired => _needRequired;

        public void Initialize(VillagerAction action)
        {
            _action = action;
            _brain = action.Brain;
        }

        public float EvaluateScore()
        {
            var score = 0.0f;

            var need = _brain.Needs.FirstOrDefault(j => j.Type.Id == _needRequired.Id);

            if (need != null && need.Value < threshold)
            {
                score += pointPerPointsBelowThreshold * (threshold - need.Value);
            }

            return score;
        }
    }
}
