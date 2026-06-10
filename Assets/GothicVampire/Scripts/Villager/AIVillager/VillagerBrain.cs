using GothicVampire.Buildings;
using GothicVampire.Game;
using GothicVampire.Grids;
using Sylpheed.Core;
using Sylpheed.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GothicVampire.Villagers.Actions;

namespace GothicVampire.Villagers
{
    /// <summary>
    /// All Decision making ordeals in the visual presentation occurs here.
    /// </summary>
    [RequireComponent(typeof(Villager))]
    [RequireComponent(typeof(VillagerPathfinding))]
    [RequireComponent(typeof(VillagerVisuals))]
    public class VillagerBrain : MonoBehaviour
    {
        #region Insepctor Field
        [Header("Reference")]
        [SerializeField] private Villager _villager;
        [SerializeField] private VillagerPathfinding _pathfinding;
        [SerializeField] private VillagerVisuals _villagerVisuals;
        #endregion

        [Header("Configuration")]
        [SerializeReference] private List<VillagerNeedType> _needs = new();
        [SerializeReference] private List<VillagerAction> _actions = new();

        private Queue<VillagerAction> _queuedVillagerActions = new();

        private BuildingManager _buildingManager;
        private IReadOnlyCollection<Building> _buildings;
        private VillagerAction _currentAction;

        public IReadOnlyCollection<VillagerNeedType> Needs => _needs;
        public VillagerPathfinding Pathfinding => _pathfinding;
        public Villager Villager => _villager;
        public GridSystem GridSystem { get; private set; }

        private void Awake()
        {
            // hard fail in play mode if still missing
            _villager = GetComponent<Villager>() ?? throw new System.Exception("Requires component Villager");
            _pathfinding = GetComponent<VillagerPathfinding>() ?? throw new System.Exception("Requires component VillagerPathFinding");
            _villagerVisuals = GetComponent<VillagerVisuals>() ?? throw new System.Exception("Requires component VillagerVisuals");
        }

        private void Start()
        {
            GridSystem = ServiceLocator.Get<GridSystem>();

            _buildingManager = ServiceLocator.Get<World>().Player.GetService<BuildingManager>();
            _buildings = _buildingManager.Buildings;

            Decide();
        }

        public void Initialize(VillagerData data)
        {
            data.VillagerNeeds.ForEach(x =>
            {
                _needs.Add(x.Clone());
            });

            data.VillagerActions.ForEach(x =>
            {
                var newAction = Instantiate(x);
                newAction.Initialize(Villager);
                _actions.Add(newAction);
            });


            _villagerVisuals.Initialize(Villager);
        }

        private void Update()
        {
            foreach (var need in _needs)
            {
                need.Tick(Time.deltaTime);
            }

            if (_currentAction == null && _queuedVillagerActions.Count > 0)
            {
                _currentAction = _queuedVillagerActions.Dequeue();
            }
            else if (_currentAction != null)
            {
                _currentAction.Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {

        }

        #region Public Methods

        public void Decide()
        {
            _actions = _actions.OrderByDescending(x => x.EvaluateScore()).ToList();
            
            var highestScoredAction = _actions.First();

            if(_currentAction != null)
            {
                _currentAction.Stop();
            }

            _currentAction = highestScoredAction;
            _currentAction.Start();
        }

        public void CompleteAction(VillagerAction action)
        {
            if (_currentAction != action) return;
            _currentAction = null;

            Decide();
        }

        #endregion
    }
}
