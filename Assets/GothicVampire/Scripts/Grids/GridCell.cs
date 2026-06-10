// GridCell.cs
using GothicVampire.Player.Inputs.Entity;
using UnityEngine;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Represents a single cell in a 2D grid.
    /// Plain C# serializable data container (POCO).
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        #region Fields (Inspector)

        [SerializeField] private bool _walkable = true;

        [SerializeField] private EntityType _occupantType;

        /// <summary>
        /// Temporary reference to the occupant of this cell.
        /// To be replaced once proper occupancy data exists.
        /// </summary>
        [SerializeField] private GameObject _occupant;

        /// <summary>
        /// Cached world position (center) for faster access.
        /// </summary>
        [SerializeField] private Vector3 _worldCenter;

        /// <summary>
        /// Determines if gridCell is unbuildable.
        /// </summary>
        [SerializeField] private bool _unbuildable;

        [SerializeField] private int _adjacentObstacleCount;
        #endregion

        #region Properties

        public int AdjacentObstacleCount
        {
            get => _adjacentObstacleCount;
            set => _adjacentObstacleCount = value;
        }

        public bool Walkable
        {
            get => _walkable;
            set => _walkable = value;
        }

        public GameObject Occupant
        {
            get => _occupant;
            set => _occupant = value;
        }

        public Vector3 WorldCenter
        {
            get => _worldCenter;
            set => _worldCenter = value;
        }

        public EntityType OccupantEntity
        {
            get => _occupantType;
            set => _occupantType = value;
        }

        public bool Unbuildable
        {
            get => _unbuildable;
            set => _unbuildable = value;
        }
        #endregion

        public float GetMoveCost()
        {
            // If the tile is a road, cost is cheap → preferred path
            if (OccupantEntity == EntityType.Road)
                return 0.5f;

            // Normal ground
            return 1f;
        }
    }
}
