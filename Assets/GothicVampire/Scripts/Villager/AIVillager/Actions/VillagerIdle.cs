using GothicVampire.Grids;
using UnityEngine;

namespace GothicVampire.Villagers.Actions
{
    [CreateAssetMenu(menuName = "Villager/Actions/Idle", order = 0)]
    public class VillagerIdle : VillagerAction
    {
        [SerializeField] private float _fixedScore = 0.15f;
        [Header("Idle Configuration")]
        [SerializeField] private float _MaxIdleByTime = 2.0f;
        [SerializeField] private float _MinIdleByTime = 1.0f;
        [Header("Idle Time")]
        [SerializeField] private float _curMaxIdleTime = 1.0f;
        [SerializeField] private float _curIdleTime = 0.0f;

        private bool _isIdling = false;
        private bool _idleComplete = false;
        private VillagerVisuals _villagerVisuals;

        protected override void OnInitialize(Villager villager)
        {
            base.OnInitialize(villager);
            _villagerVisuals = Villager.GetComponent<VillagerVisuals>();

        }
        protected override void OnStart()
        {
            base.OnStart();

            _villagerVisuals.UpdateAnimationState(AnimationState.Walking);

            _curMaxIdleTime = Random.Range(_MinIdleByTime, _MaxIdleByTime);

            // Set Destination
            Brain.Pathfinding.SetDestination(GetActionDestination());
            Brain.Pathfinding.EvtOnFinalNodeReached.AddListener(ActionDestinationReached);
        }

        protected override void OnComplete()
        {
            base.OnComplete();

            _curIdleTime = 0;
            _isIdling = false;
            _idleComplete = false;

            Brain.CompleteAction(this);
        }

        protected override void OnStop()
        {
            base.OnStop();

            Brain.Pathfinding.EvtOnFinalNodeReached.RemoveListener(ActionDestinationReached);
            ActionInProgress = false;
            _actionDestinationReached = false;
        }

        private void ActionDestinationReached()
        {
            _actionDestinationReached = true;
            StartIdling();
        }

        protected override void OnTick(float dt)
        { 
            base.OnTick(dt);

            if (!_actionDestinationReached)
            {
                Brain.Pathfinding.TickMovement();
                _villagerVisuals.UpdateSpriteFlip();
            }

            if (_isIdling && !_idleComplete)
            {
                _curIdleTime += dt;
                _idleComplete = _curIdleTime >= _curMaxIdleTime;
            }
            
            if(_idleComplete)
            {
                Complete();
            }
        }

        private void StartIdling()
        {
            if (!_isIdling)
            {
                _isIdling = true;
                _villagerVisuals.UpdateAnimationState(AnimationState.Idle);
            }
        }

        protected override float OnEvaluateScore()
        {
            return _fixedScore;
        }

        private void SetDestination()
        {
            Brain.Pathfinding.SetDestination(GetActionDestination());
            Brain.Pathfinding.EvtOnFinalNodeReached.AddListener(ActionDestinationReached);
        }

        private GridCoord GetActionDestination()
        {
            var gridSystem = Brain.GridSystem;
            var curCoord = gridSystem.Grid.GetCoordFromWorld(Brain.transform.position);
            return gridSystem.GetCoordFromDistance(curCoord, 5);
        }

    }
}
