using GothicVampire.Grids;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.Villagers.Actions
{
    [CreateAssetMenu(menuName = "Villager/Actions/Work", order = 0)]
    public class VillagerWork : VillagerAction
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

            _villagerVisuals.UpdateAnimationState(AnimationState.Walking);

            // Set Destination
            Brain.Pathfinding.SetPath(GetActionDestination());
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
                if (_brainRequiredNeeds.All(x => x.Value >= x.MaxValue))
                {
                    Complete();
                }
            }
        }

        protected override float OnEvaluateScore()
        {
            base.OnEvaluateScore();

            if(Villager.Job == null)
            {
                return 0f;
            }

            var needsScore = 1f;

            _requiredNeeds.ForEach(x =>
            {
                needsScore *= x.EvaluateScore();
            });

            needsScore = Mathf.Pow(needsScore, 1f / _requiredNeeds.Count);

            return needsScore;
        }

        private List<object> GetActionDestination()
        {
            var gridSystem = Brain.GridSystem;
            var startCell = gridSystem.Grid.GetCoordFromWorld(Villager.transform.position);
            List<PathResult> availablePaths = new List<PathResult>();

            foreach (var door in Villager.Job.Building.Doors)
            {
                var doorCoord = gridSystem.Grid.GetCoordFromWorld(door.position);
                availablePaths.Add(HybridPathfinder.FindPath(gridSystem, startCell, doorCoord));
            }

            // Returns the cheapest path
            var results = Villager.Job.Building.Doors
                .Select(door =>
                {
                    var doorCoord = gridSystem.Grid.GetCoordFromWorld(door.position);
                    var result = HybridPathfinder.FindPath(gridSystem, startCell, doorCoord);

                    Debug.Log($"Path {result.Path[result.Path.Count-1]} | Count: {result.Path.Count} | Cost {result.Cost}");

                    return result;
                })
                .Where(r => r.Path != null && r.Path.Count > 0)
                .OrderBy(r => r.Cost)
                .FirstOrDefault();

            return results.Path;

        }

        protected List<VillagerNeedType> GetRequiredNeedsFromBrain()
        {
            return Brain.Needs
                .Where(x => _requiredNeeds.Any(j => j.NeedRequired.Id == x.Type.Id))
                .ToList();
        }
    }
}
