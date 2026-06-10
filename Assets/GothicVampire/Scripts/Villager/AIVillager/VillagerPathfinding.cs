using GothicVampire.Buildings;
using GothicVampire.Player.Inputs.Entity;
using GothicVampire.Villagers;
using Sylpheed.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Grids
{
    public class VillagerPathfinding : MonoBehaviour
    {
        public UnityEvent EvtOnNextNode { get; } = new();
        public UnityEvent EvtOnFinalNodeReached { get; } = new();

        [Header("Settings")]
        [SerializeField] private float _speed = 1.5f;
        [SerializeField] private float _speedVariance = 0.15f;
        [SerializeField] private float _roadSpeedMultiplier = 1.25f;

        private float _finalSpeed;

        public bool HasReachedDestination { get; private set; }

        private Villager _villager;
        private GridSystem _gridSystem;

        // Unified hybrid path: contains BOTH GridCoord and EdgeNode
        private List<object> _currentHybridPath = new();

        private GridCoord _currentCoord;
        private bool _isOnRoad;
        void Start()
        {
            _gridSystem = ServiceLocator.Get<GridSystem>();
            _villager = GetComponent<Villager>();

            _finalSpeed = _speed * Random.Range(
                1f - _speedVariance,
                1f + _speedVariance
            );
        }

        public void SetDestination(GridCoord destinationCell)
        {
            if (_gridSystem == null)
                _gridSystem = ServiceLocator.Get<GridSystem>();

            GridCoord startCell = _gridSystem.Grid.GetCoordFromWorld(transform.position);

            // Unified hybrid pathfinding call
            SetPath(HybridPathfinder.FindPath(_gridSystem, startCell, destinationCell).Path);
        }

        public void SetPath(List<object> path)
        {
            _currentHybridPath = path;
            HasReachedDestination = _currentHybridPath == null || _currentHybridPath.Count == 0;
        }

        // MOVEMENT (Handles BOTH center + edge paths)
        public void TickMovement()
        {
            HasReachedDestination = false;

            if (_currentHybridPath == null || _currentHybridPath.Count == 0)
            {
                HasReachedDestination = true;
                return;
            }

            object next = _currentHybridPath[0];
            Vector3 targetPos;

            if (next is GridCoord center)
                targetPos = _gridSystem.Grid.GetWorldCenter(center);
            else
                targetPos = _gridSystem.GetWorldPosition((EdgeNode)next);

            var totalSpeed = _finalSpeed;

            totalSpeed *= _isOnRoad ? _roadSpeedMultiplier : 1f;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                totalSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                _currentHybridPath.RemoveAt(0);

                if (_currentHybridPath.Count > 0)
                {
                    Vector3 nextPos;

                    object nextNode = _currentHybridPath[0];
                    if (nextNode is GridCoord gc)
                        nextPos = _gridSystem.Grid.GetWorldCenter(gc);
                    else
                        nextPos = _gridSystem.GetWorldPosition((EdgeNode)nextNode);

                    // Fire event to sprite
                    EvtOnNextNode?.Invoke();
                }

                var checkedCoord = _gridSystem.Grid.GetCoordFromWorld(transform.position);
                
                // Updates _isOnRoad Flag everytime the villager reaches a point
                UpdateRoadState(checkedCoord);
            }

            if (_currentHybridPath.Count == 0)
            {
                HasReachedDestination = true;
                EvtOnFinalNodeReached?.Invoke();
            }
        }

        public GridCoord GetNearestUnoccupiedDoor(Building building)
        {
            var buildingGridPos = building.GridPosition;

            var gridCoord = building.Doors
                .Select(door => _gridSystem.Grid.GetCoordFromWorld(door.position))
                .Where(coord => _gridSystem.Grid.InBounds(coord))
                .Where(coord => HybridPathfinder.IsWalkable(_gridSystem, coord))
                .OrderBy(coord => GetDistanceFrom(coord))
                .FirstOrDefault();

            // In any cases no Doors are not occupied.
            if(gridCoord == Vector2.zero)
            {
                gridCoord = _gridSystem.Grid.GetCoordFromWorld(building.transform.position);
            }

            return gridCoord;
        }

        private void UpdateRoadState(GridCoord coord)
        {
            _isOnRoad = IsOnRoad(coord);
        }

        private bool IsOnRoad(GridCoord coord)
        {
            var cell = _gridSystem.Grid.Get(coord);
            return cell?.OccupantEntity == EntityType.Road;
        }

        private float GetDistanceFrom(GridCoord from)
        {
            var fromPosition = _gridSystem.GetWorldPosition(from);

            return Vector3.Distance(fromPosition, transform.position);
        }

        // GIZMOS (Hybrid visualization)
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _currentHybridPath == null)
                return;

            for (int i = 0; i < _currentHybridPath.Count; i++)
            {
                object node = _currentHybridPath[i];
                Vector3 pos;

                if (node is GridCoord gc)
                {
                    pos = _gridSystem.Grid.GetWorldCenter(gc);
                    Gizmos.color = Color.yellow; // center node
                }
                else
                {
                    pos = _gridSystem.GetWorldPosition((EdgeNode)node);
                    Gizmos.color = Color.magenta; // edge node
                }

                Gizmos.DrawSphere(pos, 0.1f);

                // Draw link
                if (i < _currentHybridPath.Count - 1)
                {
                    object next = _currentHybridPath[i + 1];
                    Vector3 nextPos = (next is GridCoord gc2)
                        ? _gridSystem.Grid.GetWorldCenter(gc2)
                        : _gridSystem.GetWorldPosition((EdgeNode)next);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(pos, nextPos);
                }
            }
        }
#endif

    }
}
