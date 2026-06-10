using GothicVampire.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.Villagers.Actions
{
    [CreateAssetMenu(menuName = "Villager/Actions/Rest", order = 0)]
    [Serializable]
    public class VillagerRest : VillagerAction
    {
        [Header("Needs Required")]
        [SerializeField] private List<ActionNeeds> _requiredNeeds = new();

        private List<VillagerNeedType> _brainRequiredNeeds = new();
        private VillagerVisuals _villagerVisuals;

        protected override void OnInitialize(Villager villager)
        {
            base.OnInitialize(villager);

            _villagerVisuals = Villager.GetComponent<VillagerVisuals>();
            _requiredNeeds.ForEach(x => x.Initialize(this));
            _brainRequiredNeeds = GetRequiredNeedsFromBrain();
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Set Destination
            Brain.Pathfinding.SetDestination(GetActionDestination());
            Brain.Pathfinding.EvtOnFinalNodeReached.AddListener(ActionDestinationReached);
        }

        protected override void OnComplete()
        {
            base.OnComplete();

            _villagerVisuals.SpriteRenderer.enabled = true;
            Brain.CompleteAction(this);

            _brainRequiredNeeds.ForEach(x => x.IsRecharging = false);
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
            _brainRequiredNeeds.ForEach(x => x.IsRecharging = true);
            _villagerVisuals.SpriteRenderer.enabled = false;
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);

            if (!_actionDestinationReached)
            {
                Brain.Pathfinding.TickMovement();
                _villagerVisuals.UpdateSpriteFlip();
            }
            else
            {                
                // If All needs are fully recharged.
                if(_brainRequiredNeeds.All(x => x.Value >= x.MaxValue))
                {
                    Complete();
                }
            }
        }

        protected override float OnEvaluateScore()
        {
            base.OnEvaluateScore();

            if (Villager.Job == null)
            {
                return 0f;
            }

            var needsScore = 1.0f;

            if (_requiredNeeds.Count > 0)
            {
                _requiredNeeds.ForEach(x =>
                {
                    needsScore *= x.EvaluateScore();
                });
            }

            needsScore = Mathf.Pow(needsScore, 1f / _requiredNeeds.Count);


            return needsScore;
        }

        private GridCoord GetActionDestination()
        {
            return Brain.Pathfinding.GetNearestUnoccupiedDoor(Villager.Source.Building);
        }

        protected List<VillagerNeedType> GetRequiredNeedsFromBrain()
        {
            return Brain.Needs
                .Where(x => _requiredNeeds.Any(j => j.NeedRequired.Id == x.Type.Id))
                .ToList();
        }
    }
}
