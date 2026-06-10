using System;
using UnityEngine;

namespace GothicVampire.Villagers.Actions
{
    [Serializable]
    public class VillagerAction : ScriptableObject
    {
        [Header("Evaluation Configuration")]
        [SerializeField] private float _scoreModifier = 0.25f;

        public Villager Villager { get; private set; }
        public VillagerBrain Brain { get; private set; }

        protected bool ActionInProgress { get; set; }
        protected bool _actionDestinationReached = false;
        private string _originalName;
        protected virtual float OnEvaluateScore() { return 1f; }
        protected virtual void OnInitialize(Villager villager) { }
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnComplete() { }
        protected virtual void OnTick(float dt) { }

        public float EvaluateScore()
        {
            var score = Mathf.Clamp01(OnEvaluateScore() * _scoreModifier);

            this.name = _originalName + $"[{score}]";

            return score;
        }
        
        public void Initialize(Villager villager)
        {
            _originalName = new string(name);

            Villager = villager;
            Brain = villager.GetComponent<VillagerBrain>();

            OnInitialize(villager);
        }

        /// <summary>
        /// used when you want the action to start
        /// </summary>
        public void Start() 
        {
            if (Brain == null) return;
            ActionInProgress = true;

            OnStart();
        }

        /// <summary>
        /// Used when you want the action to stop
        /// </summary>
        public void Stop() 
        {
            OnStop();
        }

        /// <summary>
        /// Used when the action is complete.
        /// </summary>
        public void Complete() 
        {
            Stop();
            OnComplete();
        }

        public void Tick(float dt) 
        {
            OnTick(dt);
        }
    }
}
